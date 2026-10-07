using ATLASDomain.Entities;
using ATLASDomain.InterFaces;
using Microsoft.Extensions.Options;

namespace ATLAS.Middleware.Alert
{
    /// <summary>
    /// Discord Notification Provider.
    /// </summary>
    public class DiscordNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// _http Client.
        /// </summary>
        private readonly HttpClient _httpClient;
        /// <summary>
        /// _options.
        /// </summary>
        private readonly DiscordOptions _options;
        /// <summary>
        /// _logger.
        /// </summary>
        private readonly ILogger<DiscordNotificationProvider> _logger;
        /// <summary>
        /// Name.
        /// </summary>
        public string Name => "Discord";

        public DiscordNotificationProvider(HttpClient httpClient, IOptions<DiscordOptions> options, ILogger<DiscordNotificationProvider> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Send Async.
        /// </summary>
        public async Task SendAsync(
    SecurityEvent securityEvent,
    CancellationToken cancellationToken)
        {
            if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.WebhookUrl))
                return;

            var webhookUrl = _options.WebhookUrl;




            string Sanitize(string? text) =>
                string.IsNullOrEmpty(text)
                    ? "Unknown"
                    : text.Replace("@everyone", "@\u200beveryone")
                          .Replace("@here", "@\u200bhere")
                          .Replace("`", "'");

            var payload = new
            {
                content =
                    $"🚨 **{Sanitize(securityEvent.Severity)} Security Event: {Sanitize(securityEvent.EventType)}**\n" +
                    $"User: {Sanitize(securityEvent.UserId?.ToString() ?? securityEvent.AttemptedAcademicId)}\n" +
                    $"IP: {Sanitize(securityEvent.IpAddress)}\n" +
                    $"Time: {securityEvent.TimestampUtc:u}\n" +
                    $"Correlation: {Sanitize(securityEvent.CorrelationId)}"
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

                    throw new HttpRequestException($"Discord webhook failed with status code {(int)response.StatusCode}: {response.ReasonPhrase}");
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
