
namespace ATLASDomain.DTO.CourseGrade;

/// <summary>
/// My Rank Dto.
/// </summary>
public class MyRankDto
{
    /// <summary>
    /// Student Id.
    /// </summary>
    public int StudentId { get; set; }
    /// <summary>
    /// Student Code.
    /// </summary>
    public string StudentCode { get; set; } = string.Empty;
    /// <summary>
    /// Full Name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    /// <summary>
    /// Total Degree.
    /// </summary>
    public decimal? TotalDegree { get; set; }
    /// <summary>
    /// Rank.
    /// </summary>
    public long? Rank { get; set; }
    
}

