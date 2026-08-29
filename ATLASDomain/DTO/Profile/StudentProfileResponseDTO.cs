

namespace ATLASDomain.DTO.Profile
{
    public class StudentProfileResponseDTO
    {
        public int StudentProfileID { get; set; }

        public string? AcademicId { get; set; }
 


        public string? FullName { get; set; } = string.Empty;
        public string? LinkedInUrl { get; set; }
        public int? SectionNumber { get; set; }
        public string Faculty { get; set; } = string.Empty;

        public string? QRCodeUrl { get; set; }
    }
}
