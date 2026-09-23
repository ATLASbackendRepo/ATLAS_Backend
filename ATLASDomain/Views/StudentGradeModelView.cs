

namespace ATLASDomain.Views
{
    /// <summary>
    /// Student Grade Model View.
    /// </summary>
    public class StudentGradeModelView
    {
        /// <summary>
        /// Module ID.
        /// </summary>
        public int    ModuleID { get; set; }
        /// <summary>
        /// Course Offering ID.
        /// </summary>
        public int CourseOfferingID { get; set; }
        /// <summary>
        /// Student Code.
        /// </summary>
        public string StudentCode { get; set; } = string.Empty;
        /// <summary>
        /// Academic ID.
        /// </summary>
        public string AcademicID { get; set; } = string.Empty;
        /// <summary>
        /// Full Name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Student Degree.
        /// </summary>
        public decimal StudentDegree { get; set; }

    }
}
