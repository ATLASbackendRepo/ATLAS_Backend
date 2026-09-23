using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.InterFaces
{
    /// <summary>
    /// ISecurity Event Service.
    /// </summary>
    public interface ISecurityEventService
    {
        Task LogAsync(SecurityEventEntry entry, CancellationToken cancellationToken = default);
        Task<int> CountRecentFailedLoginsAsync(string academicId, TimeSpan window, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Security Event Entry.
    /// </summary>
    public class SecurityEventEntry
    {
        /// <summary>
        /// Event Type.
        /// </summary>
        public string EventType { get; set; } = null!;
        /// <summary>
        /// User Id.
        /// </summary>
        public int? UserId { get; set; }
        /// <summary>
        /// Attempted Academic Id.
        /// </summary>
        public string? AttemptedAcademicId { get; set; }
        /// <summary>
        /// Severity.
        /// </summary>
        public string Severity { get; set; } = "Low";
        /// <summary>
        /// Ip Address.
        /// </summary>
        public string? IpAddress { get; set; }
        /// <summary>
        /// Description.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Correlation Id.
        /// </summary>
        public string? CorrelationId { get; set; }
    }
}
