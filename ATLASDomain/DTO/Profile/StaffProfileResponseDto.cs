
namespace ATLASDomain.DTO.Profile
{
    public class StaffProfileResponseDto
    {
        public int StaffProfileID { get; set; }

        public string AcademicId { get; set; } 
        public string Role { get; set; } = string.Empty;


        public string FullName { get; set; } = string.Empty; 
        public int? DepartmentID { get; set; }
        public string? TeachingYears { get; set; }
    
    }
}