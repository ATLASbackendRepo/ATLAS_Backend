using ATLASDomain.DTO.CourseGrade;

/// <summary>
/// Course Grades Export Dto.
/// </summary>
public class CourseGradesExportDto
{
    /// <summary>
    /// Enrollment Id.
    /// </summary>
    public int EnrollmentId { get; set; }

    /// <summary>
    /// Student Id.
    /// </summary>
    public int StudentId { get; set; }

    /// <summary>
    /// Student Name.
    /// </summary>
    public string StudentName { get; set; } = string.Empty;

    /// <summary>
    /// Student Code.
    /// </summary>
    public string StudentCode { get; set; } = string.Empty;

    /// <summary>
    /// Academic Id.
    /// </summary>
    public string AcademicId { get; set; } = string.Empty;

    /// <summary>
    /// Grades.
    /// </summary>
    public List<EXportStudentModuleGradeDto> Grades { get; set; } = new();
}
/// <summary>
/// EXport Student Module Grade Dto.
/// </summary>
public class EXportStudentModuleGradeDto
{
    /// <summary>
    /// Module Id.
    /// </summary>
    public int ModuleId { get; set; }

    /// <summary>
    /// Module Name.
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// Max Degree.
    /// </summary>
    public double MaxDegree { get; set; }

    /// <summary>
    /// Student Degree.
    /// </summary>
    public double? StudentDegree { get; set; }
}
