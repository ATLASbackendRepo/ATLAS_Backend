using ATLASDomain.Entities;

namespace ATLASDomain.DTO.Staff
{
    /// <summary>
    /// Create_Update_Staff Dto.
    /// </summary>
    public class Create_Update_StaffDto
    {
        /// <summary>
        /// Full Name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Academic Id.
        /// </summary>
        public string AcademicId { get; set; } = string.Empty;
        /// <summary>
        /// Department Id.
        /// </summary>
        public int DepartmentId { get; set; }
        /// <summary>
        /// Role.
        /// </summary>
        public enUserRole Role { get; set; }
        /// <summary>
        /// Institute Id.
        /// </summary>
        public int InstituteId { get; set; }
    }
    
    public class CreateStaffDto
    {
        /// <summary>
        /// Full Name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Academic Id.
        /// </summary>
        public string AcademicId { get; set; } = string.Empty;
        /// <summary>
        /// Department Id.
        /// </summary>
        public int DepartmentId { get; set; }
        /// <summary>
        /// Role.
        /// </summary>
        public enUserRole Role { get; set; }
        /// <summary>
        /// Institute Id.
        /// </summary>
        public int InstituteId { get; set; }
        public  string Password {get; set;} = string.Empty;
    }
}
