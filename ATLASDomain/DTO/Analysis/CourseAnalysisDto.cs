namespace ATLASDomain.DTO.Analysis
{
    /// <summary>
    /// Module Average Dto.
    /// </summary>
    public class ModuleAverageDto
{
    /// <summary>
    /// Module ID.
    /// </summary>
    public int ModuleID { get; set; }
    /// <summary>
    /// Module Name.
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;
    /// <summary>
    /// Module Avg.
    /// </summary>
    public double ModuleAvg { get; set; }
    /// <summary>
    /// Max Degree.
    /// </summary>
    public double MaxDegree { get; set; }
}


/// <summary>
/// Progress Dto.
/// </summary>
public class ProgressDto
{
    /// <summary>
    /// Module ID.
    /// </summary>
    public int ModuleID { get; set; }
    /// <summary>
    /// Module Name.
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;
    /// <summary>
    /// Graded Count.
    /// </summary>
    public int GradedCount { get; set; }
    /// <summary>
    /// Total Students.
    /// </summary>
    public int TotalStudents { get; set; }
}


/// <summary>
/// Distribution Dto.
/// </summary>
public class DistributionDto
{
    /// <summary>
    /// Range0_10.
    /// </summary>
    public int Range0_10 { get; set; }
    /// <summary>
    /// Range11_20.
    /// </summary>
    public int Range11_20 { get; set; }
    /// <summary>
    /// Range21_30.
    /// </summary>
    public int Range21_30 { get; set; }
    /// <summary>
    /// Range31_40.
    /// </summary>
    public int Range31_40 { get; set; }
    /// <summary>
    /// Class Avg.
    /// </summary>
    public double ClassAvg { get; set; }
}


/// <summary>
/// Course Analysis Response Dto.
/// </summary>
public class CourseAnalysisResponseDto
{
    /// <summary>
    /// Module Averages.
    /// </summary>
    public List<ModuleAverageDto> ModuleAverages { get; set; } = new();
    /// <summary>
    /// Progress List.
    /// </summary>
    public List<ProgressDto> ProgressList { get; set; } = new();
    /// <summary>
    /// Distribution.
    /// </summary>
    public DistributionDto Distribution { get; set; } = new();
}

}
