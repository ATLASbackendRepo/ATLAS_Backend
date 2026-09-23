namespace ATLASDomain.DTO.Enrollment
{
    /// <summary>
    /// Course Enrollment Summary Dto.
    /// </summary>
    public class CourseEnrollmentSummaryDto
    {
        /// <summary>
        /// Course Offering ID.
        /// </summary>
        public int CourseOfferingID { get; set; }
        /// <summary>
        /// Course ID.
        /// </summary>
        public int CourseID { get; set; }
        /// <summary>
        /// Course Name.
        /// </summary>
        public string CourseName { get; set; } = string.Empty;
        /// <summary>
        /// Hours.
        /// </summary>
        public int Hours { get; set; } 

        /// <summary>
        /// Doctors.
        /// </summary>
        public List<string> Doctors { get; set; } = new();
        /// <summary>
        /// Teaching Assistants.
        /// </summary>
        public List<string> TeachingAssistants { get; set; } = new();

        /// <summary>
        /// Students Count.
        /// </summary>
        public int StudentsCount { get; set; }
        /// <summary>
        /// Enrollment IDs.
        /// </summary>
        public List<int> EnrollmentIDs { get; set; } = new();
    }
}
