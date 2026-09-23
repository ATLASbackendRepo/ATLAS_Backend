namespace ATLASDomain.Entities
{
    /// <summary>
    /// Course Grade.
    /// </summary>
    public class CourseGrade
    {
        /// <summary>
        /// CG_ID.
        /// </summary>
        public int CG_ID { get; set; }
        /// <summary>
        /// Enrollment ID.
        /// </summary>
        public int EnrollmentID { get; set; }
        /// <summary>
        /// Module ID.
        /// </summary>
        public int ModuleID { get; set; }
        /// <summary>
        /// Student Degree.
        /// </summary>
        public decimal StudentDegree { get; set; }
        /// <summary>
        /// Created By.
        /// </summary>
        public int CreatedBy { get; set; }
        /// <summary>
        /// Updated By.
        /// </summary>
        public int? UpdatedBy { get; set; }


        /// <summary>
        /// Enrollment.
        /// </summary>
        public Enrollment? Enrollment { get; set; }
        /// <summary>
        /// Assessment Module.
        /// </summary>
        public AssessmentModule? AssessmentModule { get; set; }

    }
}
