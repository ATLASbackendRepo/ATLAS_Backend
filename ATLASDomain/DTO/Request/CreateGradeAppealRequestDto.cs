using System.ComponentModel.DataAnnotations;


namespace ATLASDomain.DTO.Request;

/// <summary>
/// Create Grade Appeal Request Dto.
/// </summary>
public class CreateGradeAppealRequestDto
{
    /// <summary>
    /// Course Offering ID.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int CourseOfferingID { get; set; }

    /// <summary>
    /// Assessment Module ID.
    /// </summary>
    public int? AssessmentModuleID { get; set; }

    /// <summary>
    /// Doctor User ID.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int DoctorUserID { get; set; }

    /// <summary>
    /// Current Grade.
    /// </summary>
    [Range(0, 100)]
    public decimal? CurrentGrade { get; set; }

    /// <summary>
    /// Problem Description.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public required string ProblemDescription { get; set; }
}
