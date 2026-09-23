namespace ATLASDomain.DTO.Course
{
    /// <summary>
    /// Update Course DTO.
    /// </summary>
    public class UpdateCourseDTO
    {
        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }
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
    }
}
