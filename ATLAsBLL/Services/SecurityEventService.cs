using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services
{
    public class SecurityEventService : ISecurityEventService
    {
        private readonly IDbContextFactory<AtLASDbContext> _contextFactory;
        private readonly ILogger<SecurityEventService> _logger;

        public SecurityEventService(IDbContextFactory<AtLASDbContext> contextFactory, ILogger<SecurityEventService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task LogAsync(SecurityEventEntry entry, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

                context.Set<SecurityEvent>().Add(new SecurityEvent
                {
                    EventType = entry.EventType,
                    UserId = entry.UserId,
                    AttemptedAcademicId = entry.AttemptedAcademicId,
                    Severity = entry.Severity,
                    IpAddress = entry.IpAddress,
                    Description = entry.Description,
                    CorrelationId = entry.CorrelationId,
                    TimestampUtc = DateTime.UtcNow
                });

                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // فشل تسجيل الحدث الأمني نفسه لازم يتسجل في اللوج العادي على الأقل
                _logger.LogError(ex, "Failed to write security event: {EventType}", entry.EventType);
            }
        }
        public async Task<int> CountRecentFailedLoginsAsync(string academicId, TimeSpan window, CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var since = DateTime.UtcNow - window;

            return await context.Set<SecurityEvent>()
                .Where(e => e.EventType == "FailedLogin"
                         && e.AttemptedAcademicId == academicId
                         && e.TimestampUtc >= since)
                .CountAsync(cancellationToken);
        }


    }
}
