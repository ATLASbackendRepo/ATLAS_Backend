using System.ComponentModel.DataAnnotations;


namespace ATLASDomain.DTO.Request;

/// <summary>
/// Create Grade Appeal Request Dto.
/// </summary>
public class CreateGradeAppealRequestDto
{
    [Range(1, int.MaxValue)]
    /// <summary>
    /// Course Offering ID.
    /// </summary>
    public int CourseOfferingID { get; set; }

    /// <summary>
    /// Assessment Module ID.
    /// </summary>
    public int? AssessmentModuleID { get; set; }

    [Range(1, int.MaxValue)]
    /// <summary>
    /// Doctor User ID.
    /// </summary>
    public int DoctorUserID { get; set; }

    [Range(0, 100)]
    /// <summary>
    /// Current Grade.
    /// </summary>
    public decimal? CurrentGrade { get; set; }

    [Required]
    [MaxLength(2000)]
    /// <summary>
    /// Problem Description.
    /// </summary>
    public required string ProblemDescription { get; set; }
}
