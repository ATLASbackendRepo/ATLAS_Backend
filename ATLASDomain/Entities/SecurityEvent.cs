using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.Entities
{

    /// <summary>
    /// Security Event.
    /// </summary>
    public class SecurityEvent
    {
        /// <summary>
        /// Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Event Type.
        /// </summary>
        public string EventType { get; set; } = null!;   // "FailedLogin", "UnauthorizedAccess", "MultipleFailedLogins"...

        /// <summary>
        /// User Id.
        /// </summary>
        public int? UserId { get; set; }                  // ممكن تكون null لو اليوزرنيم أصلاً مش موجود
        /// <summary>
        /// Attempted Academic Id.
        /// </summary>
        public string? AttemptedAcademicId { get; set; }     // اليوزرنيم اللي اتكتب في محاولة الدخول

        /// <summary>
        /// Severity.
        /// </summary>
        public string Severity { get; set; } = "Low";      // Low / Medium / High / Critical

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

        /// <summary>
        /// Timestamp Utc.
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Is Alerted.
        /// </summary>
        public bool IsAlerted { get; set; } = false;
        /// <summary>
        /// Alerted At Utc.
        /// </summary>
        public DateTime? AlertedAtUtc { get; set; }

        /// <summary>
        /// How many delivery attempts have been made for this event (MED-11).
        /// </summary>
        public int DeliveryAttempts { get; set; }

        /// <summary>
        /// Earliest time the next delivery attempt may run. Set after a failed
        /// attempt so a permanently broken provider is retried with a backoff
        /// instead of being retried every poll.
        /// </summary>
        public DateTime? NextAttemptAtUtc { get; set; }

        /// <summary>
        /// Message of the most recent failed delivery attempt, kept for diagnosis.
        /// </summary>
        public string? LastDeliveryError { get; set; }
    }
}
