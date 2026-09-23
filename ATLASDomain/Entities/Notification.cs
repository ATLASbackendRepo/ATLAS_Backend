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
        /// <summary>
        /// Notification ID.
        /// </summary>
        public int NotificationID { get; set; }

        /// <summary>
        /// Recipient User ID.
        /// </summary>
        public int RecipientUserID { get; set; }

        /// <summary>
        /// Type.
        /// </summary>
        public string Type { get; set; } = string.Empty; // e.g. "CourseGroupMessage", "GradeAppealStatus"

        /// <summary>
        /// Title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Body.
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// Related Message ID.
        /// </summary>
        public int? RelatedMessageID { get; set; }

        /// <summary>
        /// Is Read.
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Created At.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// Recipient User.
        /// </summary>
        public User RecipientUser { get; set; } = null!;

        /// <summary>
        /// Related Message.
        /// </summary>
        public CourseGroupMessage? RelatedMessage { get; set; }
    }
}
