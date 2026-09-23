

namespace ATLASDomain.DTO.Profile
{
    /// <summary>
    /// Student Update Profile Info DTO.
    /// </summary>
    public  class StudentUpdateProfileInfoDTO
    {
        /// <summary>
        /// Full Name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Linked In Url.
        /// </summary>
        public string? LinkedInUrl { get; set; }
        /// <summary>
        /// Section Number.
        /// </summary>
        public int SectionNumber { get; set; }
        /// <summary>
        /// Faculty.
        /// </summary>
        public string Faculty { get; set; } = string.Empty;
    }
}
