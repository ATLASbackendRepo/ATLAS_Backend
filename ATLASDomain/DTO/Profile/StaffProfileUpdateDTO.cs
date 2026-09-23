

namespace ATLASDomain.DTO.Profile
{
    /// <summary>
    /// Staff Profile Update DTO.
    /// </summary>
    public class StaffProfileUpdateDTO
    {
        /// <summary>
        /// Full Name.
        /// </summary>
        public string? FullName { get; set; } 
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
