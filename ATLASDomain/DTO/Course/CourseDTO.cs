namespace ATLASDomain.DTO.Course
{
    /// <summary>
    /// Course DTO.
    /// </summary>
    public  class CourseDTO
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
