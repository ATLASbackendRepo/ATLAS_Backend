using ATLASDomain.Entities;
using System;

namespace ATLAS.Domain.Entities
{
    /// <summary>
    /// Generic notification entity. "Type" + nullable "RelatedMessageID" keep it
    /// reusable for future notification sources (GradeAppealRequest status change, etc.),
    /// not just CourseGroupMessage.
    /// </summary>
    public class Notification
    {
        public int NotificationID { get; set; }

        public int RecipientUserID { get; set; }

        public string Type { get; set; } = string.Empty; // e.g. "CourseGroupMessage", "GradeAppealStatus"

        public string Title { get; set; } = string.Empty;

        public string? Body { get; set; }

        public int? RelatedMessageID { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User RecipientUser { get; set; } = null!;

        public CourseGroupMessage? RelatedMessage { get; set; }
    }
}