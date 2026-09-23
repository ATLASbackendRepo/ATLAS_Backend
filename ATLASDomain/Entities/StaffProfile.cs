
using ATLASDomain.Entities;

namespace ATLAS.Domain.Entities
{
    /// <summary>
    /// Staff Profile.
    /// </summary>
    public class StaffProfile
    {
        /// <summary>
        /// Staff Profile ID.
        /// </summary>
        public int? StaffProfileID { get; set; }

        /// <summary>
        /// User ID.
        /// </summary>
        public int? UserID { get; set; }              
        /// <summary>
        /// User.
        /// </summary>
        public User? User { get; set; } = default!;

        /// <summary>
        /// Academic Id.
        /// </summary>
        public string? AcademicId { get; set; }

        /// <summary>
        /// Role.
        /// </summary>
        public string? Role { get; set; }              
        /// <summary>
        /// Department ID.
        /// </summary>
        public int? DepartmentID { get; set; }
        /// <summary>
        /// Teaching Years.
        /// </summary>
        public string? TeachingYears { get; set; }    

        /// <summary>
        /// Department.
        /// </summary>
        public Department? Department { get; set; } = default!;
        /// <summary>
        /// Created At.
        /// </summary>
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Updated At.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
