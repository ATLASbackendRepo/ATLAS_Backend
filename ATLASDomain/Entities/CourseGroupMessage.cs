using ATLASDomain.Entities;
using System;
using System.Collections.Generic;

namespace ATLAS.Domain.Entities
{
    /// <summary>
    /// Represents a message sent inside a course's group.
    /// Only the assigned Doctor or TA of the CourseOffering can create these.
    /// Students have read-only access, enforced at the Application layer.
    /// </summary>
    public class CourseGroupMessage
    {
        /// <summary>
        /// Message ID.
        /// </summary>
        public int MessageID { get; set; }

        /// <summary>
        /// Course Offering ID.
        /// </summary>
        public int CourseOfferingID { get; set; }

        /// <summary>
        /// Sender User ID.
        /// </summary>
        public int SenderUserID { get; set; }

        /// <summary>
        /// Content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Sent At.
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Is Pinned.
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Is Deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation properties
        /// <summary>
        /// Sender User.
        /// </summary>
        public User SenderUser { get; set; } = null!;

        // Replace with your actual CourseOffering entity if it exists separately from Courses
        // public CourseOffering CourseOffering { get; set; } = null!;

        /// <summary>
        /// Course Offering.
        /// </summary>
        public CourseOffering CourseOffering { get; set; } = null!; 
        /// <summary>
        /// Read Receipts.
        /// </summary>
        public ICollection<CourseGroupMessageRead> ReadReceipts { get; set; } = new List<CourseGroupMessageRead>();

        /// <summary>
        /// Notifications.
        /// </summary>
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
