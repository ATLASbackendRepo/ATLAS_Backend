
namespace ATLASDomain.Views;

/// <summary>
/// Semester Student Ranking View.
/// </summary>
public class SemesterStudentRankingView
{
    /// <summary>
    /// Student ID.
    /// </summary>
    public int StudentID { get; set; }
    /// <summary>
    /// Semester ID.
    /// </summary>
    public int SemesterID { get; set; }
    /// <summary>
    /// Academic Level.
    /// </summary>
    public int AcademicLevel { get; set; }
    /// <summary>
    /// Student Code.
    /// </summary>
    public string StudentCode { get; set; } = string.Empty;
    /// <summary>
    /// Full Name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    /// <summary>
    /// Department ID.
    /// </summary>
    public int DepartmentID { get; set; }
    /// <summary>
    /// Total Degree.
    /// </summary>
    public decimal TotalDegree { get; set; }
    /// <summary>
    /// Rank.
    /// </summary>
    public long? Rank { get; set; }
    
}
