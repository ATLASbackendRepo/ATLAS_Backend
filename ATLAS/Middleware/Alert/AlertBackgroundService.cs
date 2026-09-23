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

          
            var pendingEvents = await context.Set<SecurityEvent>()
                  .Where(e => !e.IsAlerted && (e.Severity == "Medium" || e.Severity == "High" || e.Severity == "Critical"))
                  .OrderBy(e => e.TimestampUtc)
                  .Take(20)
                  .ToListAsync(cancellationToken);
            foreach (var evt in pendingEvents)
            {
                // تعليم ذري: لو Instance تاني سبقنا وعلّمها، rowsAffected هتبقى صفر ونتخطاها
                var rowsAffected = await context.Set<SecurityEvent>()
                    .Where(e => e.Id == evt.Id && !e.IsAlerted)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.IsAlerted, true)
                        .SetProperty(e => e.AlertedAtUtc, DateTime.UtcNow),
                        cancellationToken);

                if (rowsAffected == 0)
                {
                    continue; // Instance تاني بالفعل معالجها
                }

                foreach (var provider in providers)
                {
                    try
                    {
                        await provider.SendAsync(evt, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // فشل provider واحد (Discord مثلاً) ميوقفش الباقي (Email)
                        _logger.LogError(ex, "Notification provider {Provider} failed for event {EventId}", provider.Name, evt.Id);
                    }
                }
            }
        }
    }
}
