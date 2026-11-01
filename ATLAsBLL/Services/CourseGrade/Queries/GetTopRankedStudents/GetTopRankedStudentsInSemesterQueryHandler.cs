using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Interfaces.Repositories;
using MediatR;
using ATLAsBLL.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseGrade.Queries;

/// <summary>
/// Get Top Ranked Students In Semester Query Handler.
/// </summary>
public class GetTopRankedStudentsInSemesterQueryHandler
    : IRequestHandler<GetTopRankedStudentsInSemesterQuery, List<TopRankedStudentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetTopRankedStudentsInSemesterQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<List<TopRankedStudentDto>> Handle(
        GetTopRankedStudentsInSemesterQuery request, CancellationToken cancellationToken)
    {
        int callerInstitutionId = 0;
        if (!_currentUserService.IsSAdmin)
        {
            if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
            {
                throw new ForbiddenAccessException("User does not belong to a valid institution.");
            }
            callerInstitutionId = _currentUserService.InstitutionId.Value;
        }

        // 1. Verify Semester exists and belongs to caller's institution
        var semester = await _unitOfWork.Semesters.GetByIdAsync(request.SemesterId, cancellationToken);
        if (semester == null)
        {
            throw new KeyNotFoundException($"Semester with ID '{request.SemesterId}' was not found.");
        }

        if (!_currentUserService.IsSAdmin)
        {
            if (semester.InstitutionID == null || semester.InstitutionID.Value != callerInstitutionId)
            {
                throw new ForbiddenAccessException("You are not authorized to view rankings for this semester.");
            }
        }

        // 2. Query top ranked active students
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
