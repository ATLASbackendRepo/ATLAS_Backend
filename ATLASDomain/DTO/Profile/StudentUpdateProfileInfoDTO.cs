

namespace ATLASDomain.DTO.Profile
{
    public  class StudentUpdateProfileInfoDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string? LinkedInUrl { get; set; }
        public int SectionNumber { get; set; }
        public string Faculty { get; set; } = string.Empty;
    }
}
