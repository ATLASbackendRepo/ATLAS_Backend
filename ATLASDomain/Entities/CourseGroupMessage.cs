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
        public int MessageID { get; set; }

        public int CourseOfferingID { get; set; }

        public int SenderUserID { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsPinned { get; set; }

        public bool IsDeleted { get; set; }

        // Navigation properties
        public User SenderUser { get; set; } = null!;

        // Replace with your actual CourseOffering entity if it exists separately from Courses
        // public CourseOffering CourseOffering { get; set; } = null!;

        public CourseOffering CourseOffering { get; set; } = null!; 
        public ICollection<CourseGroupMessageRead> ReadReceipts { get; set; } = new List<CourseGroupMessageRead>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}