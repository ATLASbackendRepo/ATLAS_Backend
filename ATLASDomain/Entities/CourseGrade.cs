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
        /// Row version. SQL Server maintains it and EF includes it in the WHERE
        /// clause of every update, so a grade that was changed by somebody else
        /// while this one was being edited is rejected instead of silently
        /// overwritten (MED-06).
        /// </summary>
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Grade History. Every accepted change leaves a trail of the previous
        /// and the new value, so an overwrite is always attributable.
        /// </summary>
        public ICollection<CourseGradeHistory> History { get; set; } = new List<CourseGradeHistory>();

        /// <summary>
        /// Enrollment.
        /// </summary>
        public Enrollment? Enrollment { get; set; }
        /// <summary>
        /// Assessment Module.
        /// </summary>
        public AssessmentModule? AssessmentModule { get; set; }

    }

    /// <summary>
    /// Append-only record of every change made to a <see cref="CourseGrade"/>.
    /// </summary>
    public class CourseGradeHistory
    {
        /// <summary>
        /// History Id.
        /// </summary>
        public int HistoryId { get; set; }

        /// <summary>
        /// Grade the change was applied to.
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
        /// Student Degree before the change. Null when the grade was just created.
        /// </summary>
        public decimal? PreviousDegree { get; set; }

        /// <summary>
        /// Student Degree after the change.
        /// </summary>
        public decimal NewDegree { get; set; }

        /// <summary>
        /// Account that made the change.
        /// </summary>
        public int ChangedBy { get; set; }

        /// <summary>
        /// When the change was applied (UTC).
        /// </summary>
        public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Owning grade.
        /// </summary>
        public CourseGrade? CourseGrade { get; set; }
    }
}
