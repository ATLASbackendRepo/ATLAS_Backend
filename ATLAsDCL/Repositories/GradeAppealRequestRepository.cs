using ATLAsDCL.Data;
using ATLASDomain.DTO.Request;
using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Grade Appeal Request Repository.
/// </summary>
public class GradeAppealRequestRepository : IGradeAppealRequestRepository
{
    /// <summary>
    /// _context.
    /// </summary>
    private readonly AtLASDbContext _context;

    public GradeAppealRequestRepository(AtLASDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get By Id Async.
    /// </summary>
    public async Task<GradeAppealRequestDto?> GetByIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        return await _context.GradeAppealRequests
            .AsNoTracking()
            .Where(x => x.RequestID == requestId)
            .Select(x => new GradeAppealRequestDto
            {
                RequestID = x.RequestID,
                StudentID = x.StudentID,
                InstitutionID = x.CourseOffering.InstitutionID ?? 0,

                // Change CName if your property has another name.
                CourseName = x.CourseOffering.Course.C_Name,

                AssessmentModuleName = x.AssessmentModule != null
                    ? x.AssessmentModule.ModuleName
                    : null,

                DoctorUserID = x.DoctorUserID,
                CurrentGrade = x.CurrentGrade,
                ProblemDescription = x.ProblemDescription,
                Status = x.Status,
                DoctorResponse = x.DoctorResponse,
                SubmittedAt = x.SubmittedAt,
                ReviewedAt = x.ReviewedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get By Student Id Async.
    /// </summary>
    public async Task<List<GradeAppealRequestDto>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.GradeAppealRequests
            .AsNoTracking()
            .Where(x => x.StudentID == studentId)
            .OrderByDescending(x => x.SubmittedAt)
            .Select(x => new GradeAppealRequestDto
            {
                RequestID = x.RequestID,
                StudentID = x.StudentID,
                InstitutionID = x.CourseOffering.InstitutionID ?? 0,
                CourseName = x.CourseOffering.Course.C_Name,
                AssessmentModuleName = x.AssessmentModule != null
                    ? x.AssessmentModule.ModuleName
                    : null,
                DoctorUserID = x.DoctorUserID,
                CurrentGrade = x.CurrentGrade,
                ProblemDescription = x.ProblemDescription,
                Status = x.Status,
                DoctorResponse = x.DoctorResponse,
                SubmittedAt = x.SubmittedAt,
                ReviewedAt = x.ReviewedAt
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get By Doctor Id Async.
    /// </summary>
    public async Task<List<GradeAppealRequestDto>> GetByDoctorIdAsync(int doctorUserId, CancellationToken cancellationToken = default)
    {
        return await _context.GradeAppealRequests
            .AsNoTracking()
            .Where(x => x.DoctorUserID == doctorUserId)
            .OrderByDescending(x => x.SubmittedAt)
            .Select(x => new GradeAppealRequestDto
            {
                RequestID = x.RequestID,
                StudentID = x.StudentID,
                InstitutionID = x.CourseOffering.InstitutionID ?? 0,
                CourseName = x.CourseOffering.Course.C_Name,
                AssessmentModuleName = x.AssessmentModule != null
                    ? x.AssessmentModule.ModuleName
                    : null,
                DoctorUserID = x.DoctorUserID,
                CurrentGrade = x.CurrentGrade,
                ProblemDescription = x.ProblemDescription,
                Status = x.Status,
                DoctorResponse = x.DoctorResponse,
                SubmittedAt = x.SubmittedAt,
                ReviewedAt = x.ReviewedAt
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Has Open Request Async.
    /// </summary>
    public async Task<bool> HasOpenRequestAsync(
        int studentId,
        int assessmentModuleId, CancellationToken cancellationToken = default)
    {
        return await _context.GradeAppealRequests.AnyAsync(x =>
            x.StudentID == studentId &&
            x.AssessmentModuleID == assessmentModuleId &&
            (x.Status == enGradeAppealStatus.Pending
             || x.Status == enGradeAppealStatus.UnderReview), cancellationToken);
    }

    /// <summary>
    /// Add Async.
    /// </summary>
    public async Task<int> AddAsync(
        CreateGradeAppealRequestDto dto,
        int studentId, CancellationToken cancellationToken = default)
    {
        var request = new GradeAppealRequest
        {
            StudentID = studentId,
            CourseOfferingID = dto.CourseOfferingID,
            AssessmentModuleID = dto.AssessmentModuleID,
            DoctorUserID = dto.DoctorUserID,
            CurrentGrade = dto.CurrentGrade,
            ProblemDescription = dto.ProblemDescription,
            Status = enGradeAppealStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        await _context.GradeAppealRequests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return request.RequestID;
    }

    /// <summary>
    /// Update Async.
    /// </summary>
    public async Task<bool> UpdateAsync(
        int requestId,
        int doctorUserId,
        UpdateGradeAppealRequestDto dto,
        int? institutionId = null,
        bool isSAdmin = false,
        CancellationToken cancellationToken = default)
    {
        var request = await _context.GradeAppealRequests
            .Include(x => x.CourseOffering)
            .FirstOrDefaultAsync(x => x.RequestID == requestId, cancellationToken);

        if (request is null)
            return false;

        // Tenant isolation check
        if (!isSAdmin && institutionId.HasValue)
        {
            if (request.CourseOffering != null && request.CourseOffering.InstitutionID.HasValue && request.CourseOffering.InstitutionID.Value != institutionId.Value)
            {
                throw new UnauthorizedAccessException("You do not have access to appeals outside your institution.");
            }
        }

        var isAssignedDoctor = isSAdmin || request.DoctorUserID == doctorUserId ||
            await _context.CourseOfferingDoctors.AnyAsync(cod => cod.CourseOfferingID == request.CourseOfferingID && cod.DoctorID == doctorUserId, cancellationToken);

        if (!isAssignedDoctor)
            throw new UnauthorizedAccessException("You are not assigned to review this grade appeal.");

        // MED-08 / BE-06: The appeal is a state machine. Same-state transitions on final states and
        // reopening/modifying finalized appeals are strictly forbidden.
        if (!GradeAppealStatusTransitions.IsAllowed(request.Status, dto.Status))
        {
            throw new InvalidOperationException(
                $"A grade appeal cannot move from {request.Status} to {dto.Status}.");
        }

        request.Status = dto.Status;
        request.DoctorResponse = dto.DoctorResponse;

        if (dto.Status is enGradeAppealStatus.Approved
            or enGradeAppealStatus.Rejected
            or enGradeAppealStatus.Closed)
        {
            request.ReviewedAt = DateTime.UtcNow;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("This grade appeal was changed by another user. Refresh and try again.", ex);
        }

        return true;
    }
}
