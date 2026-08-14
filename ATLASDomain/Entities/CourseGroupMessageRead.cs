using ATLASDomain.Entities;
using System;

namespace ATLAS.Domain.Entities
{
    /// <summary>
    /// Tracks which student has read which CourseGroupMessage (optional "seen" feature).
    /// Composite key: (MessageID, StudentID).
    /// </summary>
    public class CourseGroupMessageRead
    {
        public int MessageID { get; set; }

        public int StudentID { get; set; }

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public CourseGroupMessage Message { get; set; } = null!;

        public Student Student { get; set; } = null!;
    }
}