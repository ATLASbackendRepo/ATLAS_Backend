using ATLASDomain.DTO.CourseGrade;

public class CourseGradesExportDto
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string StudentCode { get; set; } = string.Empty;

    public string AcademicId { get; set; } = string.Empty;

    public List<EXportStudentModuleGradeDto> Grades { get; set; } = new();
}
public class EXportStudentModuleGradeDto
{
    public string ModuleName { get; set; } = string.Empty;

    public double MaxDegree { get; set; }

    public double? StudentDegree { get; set; }
}