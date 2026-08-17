// ATLAsBLL/Features/StaffProfiles/Queries/GetStaffProfileByUserId/StaffProfileResponseDto.cs
namespace ATLASDomain.DTO.Profile
{
    public class StaffProfileResponseDto
    {
        public int StaffProfileID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty; // جاي من الـ User navigation
        public string Role { get; set; } = string.Empty;
        public int? DepartmentID { get; set; }
        public string? TeachingYears { get; set; }
    
    }
}