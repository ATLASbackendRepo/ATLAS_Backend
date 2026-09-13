using ATLASDomain.Entities;

namespace ATLASDomain.DTO.Staff
{
    public  class StaffDto
    {
        public string FullName { get; set; } = string.Empty;
        public string AcadimicId { get; set; } = string.Empty;

        public int NumberOfCourses { get; set; }

        public string departmentname { get; set; } = string.Empty;

        public bool IsActive { get; set; } 
    }
}
