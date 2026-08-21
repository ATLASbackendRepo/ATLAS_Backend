

namespace ATLASDomain.Entities
{
    public enum enGradeAppealStatus
    {
        Pending,
        Rejected,
        Approved,
        UnderReview,
        Closed
    
    }
    public class GradeAppealRequest
    {
        public int RequestID { get; set; }

        public int StudentID { get; set; }
        public int CourseOfferingID { get; set; }
        public int? AssessmentModuleID { get; set; }
        public int DoctorUserID { get; set; }

        public decimal? CurrentGrade { get; set; }
        public string ProblemDescription { get; set; } = string.Empty;

        public enGradeAppealStatus Status { get; set; } = enGradeAppealStatus.Pending;
        public string? DoctorResponse { get; set; }

        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
   

        public Student Student { get; set; } = null!;
        public CourseOffering CourseOffering { get; set; } = null!;
        public AssessmentModule? AssessmentModule { get; set; }
        public User Doctor { get; set; } = null!;
    }
}
