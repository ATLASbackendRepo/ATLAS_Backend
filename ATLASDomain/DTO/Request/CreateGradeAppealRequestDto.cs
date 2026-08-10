using System.ComponentModel.DataAnnotations;

public class CreateGradeAppealRequestDto
{
    [Range(1, int.MaxValue)]
    public int CourseOfferingID { get; set; }

    public int? AssessmentModuleID { get; set; }

    [Range(1, int.MaxValue)]
    public int DoctorUserID { get; set; }

    [Range(0, 100)]
    public decimal? CurrentGrade { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string ProblemDescription { get; set; }
}