using ATLASDomain.Entities;

namespace ATLASDomain.DTO.Staff
{
    public class Create_Update_StaffDto
    {
        public string FullName { get; set; } = string.Empty;
        public string AcademicId { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public enUserRole Role { get; set; }
        public int InstituteId { get; set; }
    }
}
