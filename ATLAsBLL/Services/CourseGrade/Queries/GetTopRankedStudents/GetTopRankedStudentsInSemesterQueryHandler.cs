using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Interfaces.Repositories;
using MediatR;

namespace ATLAsBLL.Services.CourseGrade.Queries;

public class GetTopRankedStudentsInSemesterQueryHandler
    : IRequestHandler<GetTopRankedStudentsInSemesterQuery, List<TopRankedStudentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetTopRankedStudentsInSemesterQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TopRankedStudentDto>> Handle(
        GetTopRankedStudentsInSemesterQuery request, CancellationToken cancellationToken)
    {
        var rows = await _unitOfWork.Enrollments.GetTopRankedStudentsInSemesterAsync(
            request.SemesterId, request.AcademicLevel, request.TopCount, cancellationToken);

        return rows.Select(r => new TopRankedStudentDto
        {
            StudentId = r.StudentID,
            StudentCode = r.StudentCode,
            FullName = r.FullName,
            TotalDegree = r.TotalDegree,
            Rank = r.Rank
        }).ToList();
    }
}