using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces;
using Microsoft.EntityFrameworkCore;

namespace ATLAS.Middleware.Alert
{
    /// <summary>
    /// Alert Background Service.
    /// </summary>
    public class AlertBackgroundService : BackgroundService
    {
        /// <summary>
        /// _scope Factory.
        /// </summary>
        private readonly IServiceScopeFactory _scopeFactory;
        /// <summary>
        /// _logger.
        /// </summary>
        private readonly ILogger<AlertBackgroundService> _logger;
        /// <summary>
        /// Poll Interval.
        /// </summary>
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

        public AlertBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AlertBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Execute Async.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingAlertsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Alert processing cycle failed");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        /// <summary>
        /// Process Pending Alerts Async.
        /// </summary>
        private async Task ProcessPendingAlertsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AtLASDbContext>>();
            var providers = scope.ServiceProvider.GetServices<INotificationProvider>().ToList();

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var now = DateTime.UtcNow;

            // MED-11: the retry time is part of the filter, so an event whose backoff has
            // not elapsed yet is not loaded at all. Reading it and then discarding
            // it would defeat the backoff entirely.
            var pendingEvents = await context.Set<SecurityEvent>()
                  .Where(e => !e.IsAlerted &&
                              (e.Severity == "Medium" || e.Severity == "High" || e.Severity == "Critical") &&
                              (e.NextAttemptAtUtc == null || e.NextAttemptAtUtc <= now))
                  .OrderBy(e => e.TimestampUtc)
                  .Take(20)
                  .ToListAsync(cancellationToken);
            foreach (var evt in pendingEvents)
            {
                // MED-11: send first, mark afterwards.
            // The previous order was the opposite - the row was flagged as alerted
            // before the providers were called - so a crash, a timeout or a provider
            // outage between the flag and the send lost the alert permanently. There
            // was no way to tell "delivered" apart from "attempted", and nothing was
            // ever retried.
            //
            // Claiming the row first (with a lease rather than a boolean) keeps
            // several instances from sending the same event twice, while the
            // delivery markers below mean a failed attempt is still pending and is
            // picked up again later.

            // Take a short lease so a second instance does not process the same row
            // concurrently, but not so long that a dead instance blocks it forever.
            var leaseUntil = now.AddSeconds(PollInterval.TotalSeconds * 2);

            var claimed = await context.Set<SecurityEvent>()
                .Where(e => e.Id == evt.Id &&
                            !e.IsAlerted &&
                            (e.NextAttemptAtUtc == null || e.NextAttemptAtUtc <= now))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.AlertedAtUtc, (DateTime?)null)
                    .SetProperty(e => e.NextAttemptAtUtc, leaseUntil),
                    cancellationToken);

            if (claimed == 0)
            {
                // Another instance holds the lease for this row.
                continue;
            }

            var failures = 0;
            string? lastError = null;

            foreach (var provider in providers)
            {
                try
                {
                    await provider.SendAsync(evt, cancellationToken);
                }
                catch (Exception ex)
                {
                    // One provider failing (Discord, say) must not stop the others
                    // (Email), so the failure is counted and the loop continues.
                    _logger.LogError(ex, "Notification provider {Provider} failed for event {EventId}", provider.Name, evt.Id);
                    failures++;
                    lastError = ex.Message;
                }
            }

            if (failures == 0)
            {
                // Only now is the event considered delivered.
                await context.Set<SecurityEvent>()
                    .Where(e => e.Id == evt.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.IsAlerted, true)
                        .SetProperty(e => e.AlertedAtUtc, now)
                        .SetProperty(e => e.NextAttemptAtUtc, (DateTime?)null)
                        .SetProperty(e => e.LastDeliveryError, (string?)null),
                        cancellationToken);
            }
            else
            {
                // MED-11: the event stays unalerted so the next poll retries it,
                // with the interval growing each time so a permanently broken
                // provider cannot turn into a hot retry loop.
                var attempts = evt.DeliveryAttempts + 1;
                var backoffSeconds = Math.Min(MaxBackoffSeconds, 30 * Math.Pow(2, attempts - 1));

                await context.Set<SecurityEvent>()
                    .Where(e => e.Id == evt.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.IsAlerted, false)
                        .SetProperty(e => e.DeliveryAttempts, attempts)
                        .SetProperty(e => e.NextAttemptAtUtc, now.AddSeconds(backoffSeconds))
                        .SetProperty(e => e.LastDeliveryError, lastError),
                        cancellationToken);

                _logger.LogWarning(
                    "Alert delivery for event {EventId} failed on {FailureCount} of {ProviderCount} providers; retrying in {BackoffSeconds}s",
                    evt.Id, failures, providers.Count, backoffSeconds);
            }

            await DeleteExpiredEventsAsync(context, cancellationToken);
        }
    }

    /// <summary>
    /// Longest a permanently failing event keeps being retried (MED-11).
    /// </summary>
    private const double MaxBackoffSeconds = 3600;

    /// <summary>
    /// How long an alert is kept before it is deleted (MED-11).
    /// </summary>
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(90);

    /// <summary>
    /// Number of rows removed per retention pass.
    /// </summary>
    private const int RetentionBatchSize = 500;

    /// <summary>
    /// Deletes alerts past the retention horizon.
    /// </summary>
    /// <param name="context">Context to delete with.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// MED-11: the table is append-only and was polled every 15 seconds, so it grew
    /// without bound and every poll got slower. Retention is applied in batches so
    /// the delete never holds a long lock on a table the application is still
    /// writing to.
    /// </remarks>
    private async Task DeleteExpiredEventsAsync(
        AtLASDbContext context,
        CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - RetentionPeriod;

        var expired = await context.Set<SecurityEvent>()
            .Where(e => e.TimestampUtc < cutoff)
            .OrderBy(e => e.TimestampUtc)
            .Select(e => e.Id)
            .Take(RetentionBatchSize)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
            return;

        await context.Set<SecurityEvent>()
            .Where(e => expired.Contains(e.Id))
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation("Deleted {Count} security events past the retention horizon", expired.Count);
    }
}
}
