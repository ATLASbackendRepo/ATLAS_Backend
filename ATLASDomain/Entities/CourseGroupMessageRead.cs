using ATLASDomain.Entities;

namespace ATLAS.Domain.Entities
{
    /// <summary>
    /// Tracks which student has read which CourseGroupMessage (optional "seen" feature).
    /// Composite key: (MessageID, StudentID).
    /// </summary>
    public class CourseGroupMessageRead
    {
        /// <summary>
        /// Message ID.
        /// </summary>
        public int MessageID { get; set; }

        /// <summary>
        /// User ID.
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// Read At.
        /// </summary>
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// Message.
        /// </summary>
        public CourseGroupMessage Message { get; set; } = null!;

        /// <summary>
        /// user.
        /// </summary>
        public User user { get; set; } = null!;
    }
}
