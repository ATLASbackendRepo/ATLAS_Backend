namespace ATLASDomain.DTO.Enrollment
{
    public class CourseEnrollmentSummaryDto
    {
        public int CourseOfferingID { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int Hours { get; set; } 

        public List<string> Doctors { get; set; } = new();
        public List<string> TeachingAssistants { get; set; } = new();

        public int StudentsCount { get; set; }
        public List<int> EnrollmentIDs { get; set; } = new();
    }
}
