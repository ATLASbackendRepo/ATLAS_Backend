using ATLASDomain.Entities;

namespace ATLASDomain.DTO.Staff
{
    /// <summary>
    /// Staff Dto.
    /// </summary>
    public  class StaffDto
    {
        /// <summary>
        /// Full Name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Acadimic Id.
        /// </summary>
        public string AcadimicId { get; set; } = string.Empty;

        /// <summary>
        /// Number Of Courses.
        /// </summary>
        public int NumberOfCourses { get; set; }

        /// <summary>
        /// departmentname.
        /// </summary>
        public string departmentname { get; set; } = string.Empty;

        /// <summary>
        /// Is Active.
        /// </summary>
        public bool IsActive { get; set; } 
    }
}
