using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Interfaces.Repositories;
using ATLAsBLL.Services.CourseGrade.Queries.GetMySemesterRank;
using MediatR;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseGrade.Queries;

/// <summary>
/// Get My Semester Rank Query Handler.
/// </summary>
public class GetMySemesterRankQueryHandler : IRequestHandler<GetMySemesterRankQuery, MyRankDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMySemesterRankQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<MyRankDto?> Handle(GetMySemesterRankQuery request, CancellationToken cancellationToken)
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

        // 2. Verify Student exists, is active, and belongs to caller's institution
        var student = await _unitOfWork.Students.GetStudentByUserIDAsync(request.UserId, cancellationToken);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with ID '{request.UserId}' was not found.");
        }

        if (!_currentUserService.IsSAdmin)
        {
            if (student.InstitutionID != callerInstitutionId)
            {
                throw new ForbiddenAccessException("You are not authorized to view rankings for this student.");
            }
        }

        if (!student.IsActive)
        {
            return null; // Inactive students are excluded from active rankings
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
