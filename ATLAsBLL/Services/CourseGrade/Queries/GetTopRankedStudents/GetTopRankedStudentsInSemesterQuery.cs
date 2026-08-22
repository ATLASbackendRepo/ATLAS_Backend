using ATLASDomain.DTO.CourseGrade;
using MediatR;

namespace ATLAsBLL.Services.CourseGrade.Queries;

public record GetTopRankedStudentsInSemesterQuery(int SemesterId, int AcademicLevel, int TopCount) : IQuery<List<TopRankedStudentDto>>;