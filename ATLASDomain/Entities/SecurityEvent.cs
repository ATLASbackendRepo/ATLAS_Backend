using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.Entities
{

    public class SecurityEvent
    {
        public long Id { get; set; }

        public string EventType { get; set; } = null!;   // "FailedLogin", "UnauthorizedAccess", "MultipleFailedLogins"...

        public int? UserId { get; set; }                  // ممكن تكون null لو اليوزرنيم أصلاً مش موجود
        public string? AttemptedAcademicId { get; set; }     // اليوزرنيم اللي اتكتب في محاولة الدخول

        public string Severity { get; set; } = "Low";      // Low / Medium / High / Critical

        public string? IpAddress { get; set; }
        public string? Description { get; set; }
        public string? CorrelationId { get; set; }

        public DateTime TimestampUtc { get; set; }

        public bool IsAlerted { get; set; } = false;
        public DateTime? AlertedAtUtc { get; set; }
    }
}
