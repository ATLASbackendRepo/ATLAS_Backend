using MediatR;

namespace ATLAsBLL.Features.CourseGrades.Queries.ExportCourseGrades;

/// <summary>
/// Export Course Grades Query.
/// </summary>
public record ExportCourseGradesQuery(
    int CourseOfferingId,
    string Format
) : IRequest<CourseGradesExportResult>;

/// <summary>
/// Course Grades Export Result.
/// </summary>
public class CourseGradesExportResult
{
    /// <summary>
    /// File.
    /// </summary>
    public byte[] File { get; set; } = Array.Empty<byte>();
    /// <summary>
    /// Content Type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    /// <summary>
    /// File Name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}
