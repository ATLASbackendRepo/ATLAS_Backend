
using ATLASDomain.Entities;

namespace ATLAS.Domain.Entities
{
    /// <summary>
    /// Student Profile.
    /// </summary>
    public class StudentProfile
    {
        /// <summary>
        /// Student Profile ID.
        /// </summary>
        public int StudentProfileID { get; set; }

        /// <summary>
        /// Student ID.
        /// </summary>
        public int StudentID { get; set; }         
        /// <summary>
        /// Student.
        /// </summary>
        public Student Student { get; set; } = default!;
   
        /// <summary>
        /// Linked In Url.
        /// </summary>
        public string? LinkedInUrl { get; set; }
        /// <summary>
        /// Section Number.
        /// </summary>
        public int? SectionNumber { get; set; }

        /// <summary>
        /// Faculty.
        /// </summary>
        public string Faculty { get; set; } = string.Empty;

        /// <summary>
        /// QRCode Url.
        /// </summary>
        public string? QRCodeUrl { get; set; }

        /// <summary>
        /// Created At.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Updated At.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
