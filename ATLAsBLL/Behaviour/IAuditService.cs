using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Behaviors
{
    /// <summary>
    /// Audit Log Entry.
    /// </summary>
    public class AuditLogEntry
    {
        /// <summary>
        /// User Id.
        /// </summary>
        public int? UserId { get; set; }
        /// <summary>
        /// Action.
        /// </summary>
        public string Action { get; set; } = null!;
        /// <summary>
        /// Success.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Correlation Id.
        /// </summary>
        public string? CorrelationId { get; set; }
    }

    /// <summary>
    /// IAudit Service.
    /// </summary>
    public interface IAuditService
    {
        Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
    }
}
