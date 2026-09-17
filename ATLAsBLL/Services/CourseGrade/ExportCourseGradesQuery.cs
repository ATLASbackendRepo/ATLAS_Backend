using MediatR;

namespace ATLAsBLL.Features.CourseGrades.Queries.ExportCourseGrades;

public record ExportCourseGradesQuery(
    int CourseOfferingId,
    string Format
) : IRequest<CourseGradesExportResult>;

public class CourseGradesExportResult
{
    public byte[] File { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}