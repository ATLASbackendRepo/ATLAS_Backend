
namespace ATLASDomain.DTO.Profile
{
    /// <summary>
    /// Staff Profile Response Dto.
    /// </summary>
    public class StaffProfileResponseDto
    {
        /// <summary>
        /// Staff Profile ID.
        /// </summary>
        public int? StaffProfileID { get; set; }

        /// <summary>
        /// Academic Id.
        /// </summary>
        public string? AcademicId { get; set; } 
        /// <summary>
        /// Role.
        /// </summary>
        public string? Role { get; set; } = string.Empty;


        /// <summary>
        /// Full Name.
        /// </summary>
        public string? FullName { get; set; } = string.Empty; 
        /// <summary>
        /// Department ID.
        /// </summary>
        public int? DepartmentID { get; set; }
        /// <summary>
        /// Teaching Years.
        /// </summary>
        public string? TeachingYears { get; set; }
    
    }
}
