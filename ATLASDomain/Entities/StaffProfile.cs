
using ATLASDomain.Entities;

namespace ATLAS.Domain.Entities
{
    public class StaffProfile
    {
        public int StaffProfileID { get; set; }

        public int UserID { get; set; }              
        public User User { get; set; } = default!;

       
        public string? Role { get; set; }              
        public int? DepartmentID { get; set; }
        public string? TeachingYears { get; set; }    

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}