using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Interfaces.Repositories;
using ATLAsBLL.Services.CourseGrade.Queries.GetMySemesterRank;
using MediatR;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Services.CourseGrade.Queries;

/// <summary>
/// Get My Semester Rank Query Handler.
/// </summary>
public class GetMySemesterRankQueryHandler : IRequestHandler<GetMySemesterRankQuery, MyRankDto?>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public GetMySemesterRankQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<MyRankDto?> Handle(GetMySemesterRankQuery request, CancellationToken cancellationToken)
    {
        ATLASDomain.Entities.Student? student = await _unitOfWork.Students.GetStudentByUserIDAsync(request.UserId, cancellationToken);

        if (student is null)
        {
            throw new KeyNotFoundException($"Student with ID '{request.UserId}' was not found.");
        }

        var row = await _unitOfWork.Enrollments.GetStudentSemesterRankAsync(
            student.StudentId, request.SemesterId, cancellationToken);

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
