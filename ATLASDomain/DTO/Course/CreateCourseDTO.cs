namespace ATLASDomain.DTO.Course
{
    /// <summary>
    /// Create Course DTO.
    /// </summary>
    public class CreateCourseDTO
    {
        /// <summary>
        /// Course Name.
        /// </summary>
        public string? CourseName { get; set; }
        /// <summary>
        /// Credit Hours.
        /// </summary>
        public int CreditHours { get; set; }
        /// <summary>
        /// Is Practical.
        /// </summary>
        public bool IsPractical { get; set; }
        /// <summary>
        /// Target Institution ID (required for SAdmin, overridden by caller claims for Admin).
        /// </summary>
        public int? InstitutionId { get; set; }
    }
}
