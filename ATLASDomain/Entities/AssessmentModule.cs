namespace ATLASDomain.Entities
{
    /// <summary>
    /// Assessment Module.
    /// </summary>
    public class AssessmentModule
    {
        /// <summary>
        /// Minimum Max Degree.
        /// </summary>
        public const decimal MinimumMaxDegree = 1;
        /// <summary>
        /// Maximum Max Degree.
        /// </summary>
        public const decimal MaximumMaxDegree = 40;
        /// <summary>
        /// Module ID.
        /// </summary>
        public int ModuleID { get; set; }
        /// <summary>
        /// Course Offering ID.
        /// </summary>
        public int CourseOfferingID { get; set; }
        /// <summary>
        /// Module Name.
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// Max Degree.
        /// </summary>
        public decimal MaxDegree { get; set; }

        // Navigation Properties
        /// <summary>
        /// Course Offering.
        /// </summary>
        public CourseOffering? CourseOffering { get; set; }
        /// <summary>
        /// Course Grades.
        /// </summary>
        public ICollection<CourseGrade> CourseGrades { get; set; } = new List<CourseGrade>();
    }
}
