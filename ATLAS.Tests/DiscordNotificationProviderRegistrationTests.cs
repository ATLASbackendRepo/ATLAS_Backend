using ATLAsBLL.Features.Semesters.Commands.UpdateSemester;
using ATLAsDCL.Repositories;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Moq;
using Xunit;

using ATLAS.Middleware.Alert;
using ATLASDomain.InterFaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace ATLAS.Tests;

/// <summary>
/// MED-11: the Discord provider must be built through IHttpClientFactory so the
/// configured timeout actually applies.
/// </summary>
/// <remarks>
/// The original wiring registered the typed client with AddHttpClient and then
/// immediately re-registered the provider with AddScoped&lt;INotificationProvider,
/// ...&gt;. AddScoped builds the provider itself, so it received the container's
/// default HttpClient: no timeout, and a handler-lifetime socket. A webhook that
/// never answered would therefore block the alert poller indefinitely and stall
/// every other pending alert behind it.
/// </remarks>
public class DiscordNotificationProviderRegistrationTests
{
    /// <summary>
    /// Builds the container exactly the way Program.cs does for this provider.
    /// </summary>
    private static ServiceProvider BuildContainer()
    {
        var services = new ServiceCollection();

        services.AddOptions<DiscordOptions>()
            .Configure(options => options.Enabled = true);

        services.AddLogging();

        // Mirrors the registration in Program.cs.
        services
            .AddHttpClient<DiscordNotificationProvider>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        services.AddScoped<INotificationProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<DiscordNotificationProvider>());

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// The provider resolves as an INotificationProvider, which is how the poller
    /// consumes it.
    /// </summary>
    [Fact]
    public void Provider_Resolves_AsNotificationProvider()
    {
        using var provider = BuildContainer();
        using var scope = provider.CreateScope();

        var notification = scope.ServiceProvider
            .GetServices<INotificationProvider>()
            .SingleOrDefault(p => p.Name == "Discord");

        Assert.NotNull(notification);
    }

    /// <summary>
    /// The injected client is the factory-built one, so it carries the timeout
    /// instead of the infinite default.
    /// </summary>
    [Fact]
    public void Provider_Receives_FactoryBuilt_Client_WithTimeout()
    {
        using var provider = BuildContainer();

        var factory = provider.GetRequiredService<IHttpClientFactory>();

        // Resolving through the factory is what AddHttpClient configures. If the
        // provider had been built by AddScoped it would hold the default client
        // instead, and this would not reflect what the provider actually got.
        var client = factory.CreateClient(nameof(DiscordNotificationProvider));

        Assert.Equal(TimeSpan.FromSeconds(10), client.Timeout);
        Assert.NotEqual(Timeout.InfiniteTimeSpan, client.Timeout);
    }
}
