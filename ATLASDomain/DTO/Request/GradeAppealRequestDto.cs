using ATLASDomain.Entities;

public class GradeAppealRequestDto
{
    public int RequestID { get; set; }

     public int StudentID { get; set; }
    required public string CourseName { get; set; } = string.Empty;
    public string? AssessmentModuleName { get; set; }
     public int DoctorUserID { get; set; }

    public decimal? CurrentGrade { get; set; }
    required public string ProblemDescription { get; set; } = string.Empty;

     public enGradeAppealStatus Status { get; set; }
    public string? DoctorResponse { get; set; }

    public DateTime SubmittedAt { get; set; } 
    public DateTime? ReviewedAt { get; set; }
}