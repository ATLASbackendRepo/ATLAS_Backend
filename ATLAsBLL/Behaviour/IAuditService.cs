using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Behaviors
{
    public class AuditLogEntry
    {
        public int? UserId { get; set; }
        public string Action { get; set; } = null!;
        public bool Success { get; set; }
        public string? CorrelationId { get; set; }
    }

    public interface IAuditService
    {
        Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
    }
}