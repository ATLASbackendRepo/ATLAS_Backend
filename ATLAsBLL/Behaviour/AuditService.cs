using ATLAsBLL.Behaviors;
using ATLAsDCL.Data;
using ATLAsDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Behaviour
{
    /// <summary>
    /// Audit Service.
    /// </summary>
    public class AuditService : IAuditService
    {
        // بنستخدم Factory مش الـ Context المشترك مع الـ Unit of Work
        /// <summary>
        /// _context Factory.
        /// </summary>
        private readonly IDbContextFactory<AtLASDbContext> _contextFactory; // TODO: اسم الـ Context الحقيقي عندك
        /// <summary>
        /// _logger.
        /// </summary>
        private readonly ILogger<AuditService> _logger;

        public AuditService(IDbContextFactory<AtLASDbContext> contextFactory, ILogger<AuditService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// Log Async.
        /// </summary>
        public async Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
        {
            try
            {
                // Context جديد ومستقل، برة أي Transaction شغالة على الـ Request الأصلي
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

                context.Set<AuditLog>().Add(new AuditLog
                {
                    UserId = entry.UserId,
                    Action = entry.Action,
                    Success = entry.Success,
                    CorrelationId = entry.CorrelationId,
                    TimestampUtc = DateTime.UtcNow
                });

                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // مهم جدًا: فشل كتابة الـ Audit لازم ميكسرش الـ Request الأصلي أبدًا
                _logger.LogError(ex, "Failed to write audit log for action {Action}", entry.Action);
            }
        }
    }
}
