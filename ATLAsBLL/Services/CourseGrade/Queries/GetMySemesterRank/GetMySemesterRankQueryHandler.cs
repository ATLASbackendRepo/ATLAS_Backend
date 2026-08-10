using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Interfaces.Repositories;
using ATLAsBLL.Services.CourseGrade.Queries.GetMySemesterRank;
using MediatR;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Services.CourseGrade.Queries;

public class GetMySemesterRankQueryHandler : IRequestHandler<GetMySemesterRankQuery, MyRankDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMySemesterRankQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MyRankDto?> Handle(GetMySemesterRankQuery request, CancellationToken cancellationToken)
    {
        var row = await _unitOfWork.Enrollments.GetStudentSemesterRankAsync(
            request.StudentId, request.SemesterId, cancellationToken);

        if (row is null)
            return null;

        return new MyRankDto
        {
            StudentId = row.StudentID,
            StudentCode = row.StudentCode,
            FullName = row.FullName,
            TotalDegree = row.TotalDegree,
            Rank = row.Rank,
           
        };
    }
}