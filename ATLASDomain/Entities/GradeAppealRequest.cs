

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
    /// The allowed grade-appeal status transitions (MED-08).
    /// </summary>
    /// <remarks>
    /// An appeal is a state machine, not a free-form field. <see cref="enGradeAppealStatus.Pending"/>
    /// and <see cref="enGradeAppealStatus.UnderReview"/> are open states, while
    /// <see cref="enGradeAppealStatus.Approved"/>, <see cref="enGradeAppealStatus.Rejected"/>
    /// and <see cref="enGradeAppealStatus.Closed"/> are final. A final appeal can
    /// never be reopened and a pending appeal can only move forward, so any jump
    /// such as Closed to Approved is rejected instead of being applied.
    /// </remarks>
    public static class GradeAppealStatusTransitions
    {
        /// <summary>
        /// Reports whether an appeal has reached a state it can no longer leave.
        /// </summary>
        /// <param name="status">Status to inspect.</param>
        /// <returns>True when the status is final.</returns>
        public static bool IsFinal(enGradeAppealStatus status)
            => status is enGradeAppealStatus.Approved
                or enGradeAppealStatus.Rejected
                or enGradeAppealStatus.Closed;

        /// <summary>
        /// Decides whether an appeal may move from one status to the next.
        /// </summary>
        /// <param name="from">Current status.</param>
        /// <param name="to">Requested status.</param>
        /// <returns>True when the transition is part of the state machine.</returns>
        public static bool IsAllowed(enGradeAppealStatus from, enGradeAppealStatus to)
            => (from, to) switch
            {
                // Re-submitting the same status is always accepted.
                _ when from == to => true,

                // A pending appeal can be picked up, decided, or closed.
                (enGradeAppealStatus.Pending, enGradeAppealStatus.UnderReview) => true,
                (enGradeAppealStatus.Pending, enGradeAppealStatus.Approved) => true,
                (enGradeAppealStatus.Pending, enGradeAppealStatus.Rejected) => true,
                (enGradeAppealStatus.Pending, enGradeAppealStatus.Closed) => true,

                // An appeal under review can be decided or closed, never re-opened.
                (enGradeAppealStatus.UnderReview, enGradeAppealStatus.Approved) => true,
                (enGradeAppealStatus.UnderReview, enGradeAppealStatus.Rejected) => true,
                (enGradeAppealStatus.UnderReview, enGradeAppealStatus.Closed) => true,

                // Final states are terminal.
                _ => false
            };
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
        /// <summary>
        /// Row version (MED-08). Two doctors acting on the same appeal at the
        /// same time would otherwise silently overwrite each other's decision.
        /// </summary>
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    
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
