using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ATLAS;
using ATLAS.Controllers.Authtenication;
using ATLAsBLL.Services.Auth.Queries;
using ATLASDomain.DTO.Login;
using ATLASDomain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace ATLAS.Tests;

public class AuthenticationAndAuthorizationTests
{
    [Fact]
    public async Task StudentLogin_ValidCredentials_ReturnsTokenWithStudentClaims()
    {
        var user = new User
        {
          
            UserId = 42,
            AcademicId = "A-100",
          
            IsActive = true,
          
             
                FullName = "Test Student",
                
                Password = BCrypt.Net.BCrypt.HashPassword("correct-password")
            
        };

        var services = new ServiceCollection();
        services.AddSingleton<IRequestHandler<GetUserByAcademicIdQuery, User?>>(new StudentLookupHandler(user));
        using var provider = services.BuildServiceProvider();
        var controller = new AuthController(
            new FakeMediator(provider),
            Options.Create(new JwtOptions
            {
                Key = "test-signing-key-must-be-at-least-32-characters",
                Issuer = "ATLAS.Tests",
                Audience = "ATLAS.Tests",
                ExpiryMinutes = 30
            }));

        var result = await controller.Login(new LoginDTO
        {
            AcademicId = "A-100",
            Password = "correct-password"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var token = (string)ok.Value!.GetType().GetProperty("token")!.GetValue(ok.Value)!;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(jwt.Claims, x => x.Type == ClaimTypes.NameIdentifier && x.Value == "42");
      
        Assert.Contains(jwt.Claims, x => x.Type == ClaimTypes.Name && x.Value == "Test Student");
        Assert.Contains(jwt.Claims, x => x.Type == ClaimTypes.Role && x.Value == "Student");
        Assert.Contains(jwt.Claims, x => x.Type == "AcademicId" && x.Value == "A-100");

    }

    [Fact]
    public async Task StudentLogin_MissingCredentials_ReturnsBadRequest()
    {
        var controller = new AuthController(
            new FakeMediator(new ServiceCollection().BuildServiceProvider()),
            Options.Create(new JwtOptions()));

        var result = await controller.Login(new LoginDTO());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private sealed class StudentLookupHandler(User user)
        : IRequestHandler<GetUserByAcademicIdQuery, User?>
    {
        public Task<User?> Handle(GetUserByAcademicIdQuery request, CancellationToken cancellationToken) =>
            Task.FromResult<User?>(request.AcademicId == user.AcademicId ? user : null);
    }

    private sealed class FakeMediator : IMediator
    {
        private readonly IServiceProvider _provider;

        public FakeMediator(IServiceProvider provider) => _provider = provider;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _provider.GetService(handlerType);
            if (handler == null)
                throw new InvalidOperationException($"No handler registered for {request.GetType()}");

            var method = handlerType.GetMethod("Handle");
            return (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        // Implement the void-style Send<TRequest>(TRequest) required by ISender
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            // no-op for tests; if you need behavior, add dispatch logic similar to the generic Send above
            return Task.CompletedTask;
        }

        // Implement stream-returning members required by ISender
        public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            // return an empty stream; implement actual streaming behavior if needed by tests
            yield break;
        }

        public async IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            // return an empty stream; implement actual streaming behavior if needed by tests
            yield break;
        }

        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
    }
}
