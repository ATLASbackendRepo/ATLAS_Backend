
namespace ATLASDomain.DTO.CourseGrade;

public class MyRankDto
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal? TotalDegree { get; set; }
    public int Rank { get; set; }
    
}

