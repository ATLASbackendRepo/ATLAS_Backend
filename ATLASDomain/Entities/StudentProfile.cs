
using ATLASDomain.Entities;

namespace ATLAS.Domain.Entities
{
    public class StudentProfile
    {
        public int StudentProfileID { get; set; }

        public int StudentID { get; set; }         
        public Student Student { get; set; } = default!;

        public string? LinkedInUrl { get; set; }
        public int SectionNumber { get; set; }
        public string? Faculty { get; set; }
        public string? QRCodeUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}