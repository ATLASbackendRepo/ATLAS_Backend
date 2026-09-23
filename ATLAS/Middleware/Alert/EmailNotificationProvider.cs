    using ATLASDomain.Entities;
    using ATLASDomain.InterFaces;
    using Microsoft.Extensions.Options;
    using System.Net;
    using System.Net.Mail;
namespace ATLAS.Middleware.Alert
{

    /// <summary>
    /// Email Notification Provider.
    /// </summary>
    public class EmailNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// _options.
        /// </summary>
        private readonly EmailOptions _options;
        /// <summary>
        /// _logger.
        /// </summary>
        private readonly ILogger<EmailNotificationProvider> _logger;

        /// <summary>
        /// Name.
        /// </summary>
        public string Name => "Email";

        public EmailNotificationProvider(IOptions<EmailOptions> options, ILogger<EmailNotificationProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Send Async.
        /// </summary>
        public async Task SendAsync(SecurityEvent securityEvent, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.RecipientEmail))
            {
                _logger.LogWarning("Recipient email not configured, skipping alert for event {EventId}", securityEvent.Id);
                return;
            }

            using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.SenderEmail, _options.SenderPassword)
            };

            using var message = new MailMessage(_options.SenderEmail, _options.RecipientEmail)
            {
                Subject = $"[ATLAS Security] {securityEvent.Severity}: {securityEvent.EventType}",
                Body = $"Event: {securityEvent.EventType}\n" +
                       $"Severity: {securityEvent.Severity}\n" +
                       $"User: {securityEvent.UserId?.ToString() ?? securityEvent.AttemptedAcademicId ?? "Unknown"}\n" +
                       $"IP: {securityEvent.IpAddress}\n" +
                       $"Time: {securityEvent.TimestampUtc:u}\n" +
                       $"Correlation ID: {securityEvent.CorrelationId}\n" +
                       $"Description: {securityEvent.Description}"
            };

            await client.SendMailAsync(message, cancellationToken);
        }
    }
}
