

namespace ATLASDomain.Entities
{
    /// <summary>
    /// en Grade Appeal Status.
    /// </summary>
    public enum enGradeAppealStatus
    {
        Pending,
        Rejected,
        Approved,
        UnderReview,
        Closed
    
    }
    /// <summary>
    /// Grade Appeal Request.
    /// </summary>
    public class GradeAppealRequest
    {
        /// <summary>
        /// Request ID.
        /// </summary>
        public int RequestID { get; set; }

        /// <summary>
        /// Student ID.
        /// </summary>
        public int StudentID { get; set; }
        /// <summary>
        /// Course Offering ID.
        /// </summary>
        public int CourseOfferingID { get; set; }
        /// <summary>
        /// Assessment Module ID.
        /// </summary>
        public int? AssessmentModuleID { get; set; }
        /// <summary>
        /// Doctor User ID.
        /// </summary>
        public int DoctorUserID { get; set; }

        /// <summary>
        /// Current Grade.
        /// </summary>
        public decimal? CurrentGrade { get; set; }
        /// <summary>
        /// Problem Description.
        /// </summary>
        public string ProblemDescription { get; set; } = string.Empty;

        /// <summary>
        /// Status.
        /// </summary>
        public enGradeAppealStatus Status { get; set; } = enGradeAppealStatus.Pending;
        /// <summary>
        /// Doctor Response.
        /// </summary>
        public string? DoctorResponse { get; set; }

        /// <summary>
        /// Submitted At.
        /// </summary>
        public DateTime SubmittedAt { get; set; }  = DateTime.UtcNow;
        /// <summary>
        /// Reviewed At.
        /// </summary>
        public DateTime? ReviewedAt { get; set; }
   

        /// <summary>
        /// Student.
        /// </summary>
        public Student Student { get; set; } = null!;
        /// <summary>
        /// Course Offering.
        /// </summary>
        public CourseOffering CourseOffering { get; set; } = null!;
        /// <summary>
        /// Assessment Module.
        /// </summary>
        public AssessmentModule? AssessmentModule { get; set; }
        /// <summary>
        /// Doctor.
        /// </summary>
        public User Doctor { get; set; } = null!;
    }
}
