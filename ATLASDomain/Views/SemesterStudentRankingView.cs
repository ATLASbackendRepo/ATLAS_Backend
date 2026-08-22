
namespace ATLASDomain.Views;

public class SemesterStudentRankingView
{
    public int StudentID { get; set; }
    public int SemesterID { get; set; }
    public int AcademicLevel { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int DepartmentID { get; set; }
    public decimal TotalDegree { get; set; }
    public long? Rank { get; set; }
    
}