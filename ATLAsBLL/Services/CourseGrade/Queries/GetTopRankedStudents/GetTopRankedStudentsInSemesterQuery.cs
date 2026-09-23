using ATLASDomain.DTO.CourseGrade;
using MediatR;

namespace ATLAsBLL.Services.CourseGrade.Queries;

/// <summary>
/// Get Top Ranked Students In Semester Query.
/// </summary>
public record GetTopRankedStudentsInSemesterQuery(int SemesterId, int AcademicLevel, int TopCount) : IQuery<List<TopRankedStudentDto>>;
