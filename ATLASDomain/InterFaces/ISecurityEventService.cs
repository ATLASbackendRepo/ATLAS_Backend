using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.InterFaces
{
    public interface ISecurityEventService
    {
        Task LogAsync(SecurityEventEntry entry, CancellationToken cancellationToken = default);
        Task<int> CountRecentFailedLoginsAsync(string academicId, TimeSpan window, CancellationToken cancellationToken = default);
    }

    public class SecurityEventEntry
    {
        public string EventType { get; set; } = null!;
        public int? UserId { get; set; }
        public string? AttemptedAcademicId { get; set; }
        public string Severity { get; set; } = "Low";
        public string? IpAddress { get; set; }
        public string? Description { get; set; }
        public string? CorrelationId { get; set; }
    }
}
