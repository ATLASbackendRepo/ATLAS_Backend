

namespace ATLASDomain.Views
{
    public class StudentGradeModelView
    {
        public int    ModuleID { get; set; }
        public int CourseOfferingID { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string AcademicID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal StudentDegree { get; set; }

    }
}
