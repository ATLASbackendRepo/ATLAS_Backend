using ATLASDomain.Entities;
using ATLASDomain.InterFaces;
using Microsoft.Extensions.Options;

namespace ATLAS.Middleware.Alert
{
    public class DiscordNotificationProvider : INotificationProvider
    {
        private readonly HttpClient _httpClient;
        private readonly DiscordOptions _options;
        private readonly ILogger<DiscordNotificationProvider> _logger;
        public string Name => "Discord";

        public DiscordNotificationProvider(HttpClient httpClient, IOptions<DiscordOptions> options, ILogger<DiscordNotificationProvider> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(
    SecurityEvent securityEvent,
    CancellationToken cancellationToken)
        {
            if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.WebhookUrl))
                return;

            var webhookUrl = _options.WebhookUrl;




            var payload = new
            {
                content =
                    $"🚨 **{securityEvent.Severity} Security Event: {securityEvent.EventType}**\n" +
                    $"User: {securityEvent.UserId?.ToString() ?? securityEvent.AttemptedAcademicId ?? "Unknown"}\n" +
                    $"IP: {securityEvent.IpAddress}\n" +
                    $"Time: {securityEvent.TimestampUtc:u}\n" +
                    $"Correlation: {securityEvent.CorrelationId}"
            };

            try
            {
                _logger.LogInformation(
                    "Sending Discord alert for SecurityEvent {EventId}",
                    securityEvent.Id);

                var response = await _httpClient.PostAsJsonAsync(
                    webhookUrl,
                    payload,
                    cancellationToken);

                var responseBody =
                    await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Discord webhook failed. Status: {StatusCode}, Reason: {Reason}, Response: {Response}",
                        (int)response.StatusCode,
                        response.ReasonPhrase,
                        responseBody);

                    return;
                }

                _logger.LogInformation(
                    "Discord alert sent successfully for SecurityEvent {EventId}",
                    securityEvent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception while sending Discord alert for SecurityEvent {EventId}",
                    securityEvent.Id);

                throw;
            }
        }
    }
}
