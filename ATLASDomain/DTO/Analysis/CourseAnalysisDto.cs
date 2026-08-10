namespace ATLASDomain.DTO.Analysis
{
    public class ModuleAverageDto
{
    public int ModuleID { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public double ModuleAvg { get; set; }
    public double MaxDegree { get; set; }
}


public class ProgressDto
{
    public int ModuleID { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public int GradedCount { get; set; }
    public int TotalStudents { get; set; }
}


public class DistributionDto
{
    public int Range0_10 { get; set; }
    public int Range11_20 { get; set; }
    public int Range21_30 { get; set; }
    public int Range31_40 { get; set; }
    public double ClassAvg { get; set; }
}


public class CourseAnalysisResponseDto
{
    public List<ModuleAverageDto> ModuleAverages { get; set; } = new();
    public List<ProgressDto> ProgressList { get; set; } = new();
    public DistributionDto Distribution { get; set; } = new();
}

}