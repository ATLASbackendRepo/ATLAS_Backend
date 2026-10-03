using ATLAsDCL.Data;
using ATLASDomain.DTO.Request;
using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;

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
    public async Task<GradeAppealRequestDto?> GetByIdAsync(int requestId)
    {
        return await _context.GradeAppealRequests
            .AsNoTracking()
            .Where(x => x.RequestID == requestId)
            .Select(x => new GradeAppealRequestDto
            {
                RequestID = x.RequestID,
                StudentID = x.StudentID,

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
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get By Student Id Async.
    /// </summary>
    public async Task<List<GradeAppealRequestDto>> GetByStudentIdAsync(int studentId)
    {
        return await _context.GradeAppealRequests
            .AsNoTracking()
            .Where(x => x.StudentID == studentId)
            .OrderByDescending(x => x.SubmittedAt)
            .Select(x => new GradeAppealRequestDto
            {
                RequestID = x.RequestID,
                StudentID = x.StudentID,
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
            .ToListAsync();
    }

    /// <summary>
    /// Get By Doctor Id Async.
    /// </summary>
    public async Task<List<GradeAppealRequestDto>> GetByDoctorIdAsync(int doctorUserId)
    {
        return await _context.GradeAppealRequests
            .AsNoTracking()
            .Where(x => x.DoctorUserID   == doctorUserId)
            .OrderByDescending(x => x.SubmittedAt)
            .Select(x => new GradeAppealRequestDto
            {
                RequestID = x.RequestID,
                StudentID = x.StudentID,
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
            .ToListAsync();
    }

    /// <summary>
    /// Has Open Request Async.
    /// </summary>
    public async Task<bool> HasOpenRequestAsync(
        int studentId,
        int assessmentModuleId)
    {
        return await _context.GradeAppealRequests.AnyAsync(x =>
            x.StudentID == studentId &&
            x.AssessmentModuleID == assessmentModuleId &&
            (x.Status == enGradeAppealStatus.Pending
             || x.Status == enGradeAppealStatus.UnderReview));
    }
    /// <summary>
    /// Add Async.
    /// </summary>
    public async Task<int> AddAsync(
        CreateGradeAppealRequestDto dto,
        int studentId)
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

        await _context.GradeAppealRequests.AddAsync(request);
        await _context.SaveChangesAsync();

        return request.RequestID;
    }
    /// <summary>
    /// Update Async.
    /// </summary>
    public async Task<bool> UpdateAsync(
        int requestId,
        int doctorUserId,
        UpdateGradeAppealRequestDto dto)
    {
        var request = await _context.GradeAppealRequests
            .FirstOrDefaultAsync(x =>
                x.RequestID == requestId &&
                x.DoctorUserID == doctorUserId);

        if (request is null)
            return false;

        // MED-08: the status used to be written straight from the request body, so
        // any value could replace any other one. That let a final appeal be reopened
        // (Closed to Approved) and let a not-yet-reviewed appeal jump straight to a
        // decision. The appeal is a state machine, so a transition that is not part
        // of it is refused rather than applied.
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
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("This grade appeal was changed by another user. Refresh and try again.", ex);
        }

        return true;
    }
}
