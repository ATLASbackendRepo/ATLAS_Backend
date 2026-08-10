namespace ATLASDomain.Entities
{
    public class AssessmentModule
    {
        public int ModuleID { get; set; }
        public int CourseOfferingID { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public decimal MaxDegree { get; set; }

        // Navigation Properties
        public CourseOffering? CourseOffering { get; set; }
        public ICollection<CourseGrade> CourseGrades { get; set; } = new List<CourseGrade>();
    }
}