using ATLASDomain.Entities;
namespace ATLASDomain.DTO.Request;

/// <summary>
/// Grade Appeal Request Dto.
/// </summary>
public class GradeAppealRequestDto
{
    /// <summary>
    /// Request ID.
    /// </summary>
    public int RequestID { get; set; }

     /// <summary>
     /// Student ID.
     /// </summary>
     public int StudentID { get; set; }
    required public string CourseName { get; set; } = string.Empty;
    /// <summary>
    /// Assessment Module Name.
    /// </summary>
    public string? AssessmentModuleName { get; set; }
     /// <summary>
     /// Doctor User ID.
     /// </summary>
     public int DoctorUserID { get; set; }

    /// <summary>
    /// Current Grade.
    /// </summary>
    public decimal? CurrentGrade { get; set; }
    required public string ProblemDescription { get; set; } = string.Empty;

     /// <summary>
     /// Status.
     /// </summary>
     public enGradeAppealStatus Status { get; set; }
    /// <summary>
    /// Doctor Response.
    /// </summary>
    public string? DoctorResponse { get; set; }

    /// <summary>
    /// Submitted At.
    /// </summary>
    public DateTime SubmittedAt { get; set; } 
    /// <summary>
    /// Reviewed At.
    /// </summary>
    public DateTime? ReviewedAt { get; set; }
}
