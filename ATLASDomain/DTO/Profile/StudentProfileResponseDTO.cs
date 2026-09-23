

namespace ATLASDomain.DTO.Profile
{
    /// <summary>
    /// Student Profile Response DTO.
    /// </summary>
    public class StudentProfileResponseDTO
    {
        /// <summary>
        /// Student Profile ID.
        /// </summary>
        public int StudentProfileID { get; set; }

        /// <summary>
        /// Academic Id.
        /// </summary>
        public string? AcademicId { get; set; }
 


        /// <summary>
        /// Full Name.
        /// </summary>
        public string? FullName { get; set; } = string.Empty;
        /// <summary>
        /// Linked In Url.
        /// </summary>
        public string? LinkedInUrl { get; set; }
        /// <summary>
        /// Section Number.
        /// </summary>
        public int? SectionNumber { get; set; }
        /// <summary>
        /// Faculty.
        /// </summary>
        public string Faculty { get; set; } = string.Empty;

        /// <summary>
        /// QRCode Url.
        /// </summary>
        public string? QRCodeUrl { get; set; }
    }
}
