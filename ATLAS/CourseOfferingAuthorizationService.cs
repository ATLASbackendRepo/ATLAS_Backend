using System.Security.Claims;
using ATLAsDCL.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace ATLAS;

/// <summary>
/// ICourse Offering Authorization Service.
/// </summary>
public interface ICourseOfferingAuthorizationService
{
    Task<bool> CanAccessOfferingAsync(ClaimsPrincipal user, int courseOfferingId, CancellationToken cancellationToken = default);
    Task<bool> CanManageGradeAsync(ClaimsPrincipal user, int enrollmentId, int moduleId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Course Offering Authorization Service.
/// </summary>
public sealed class CourseOfferingAuthorizationService(AtLASDbContext context) : ICourseOfferingAuthorizationService
{
    /// <summary>
    /// Can Access Offering Async.
    /// </summary>
    public async Task<bool> CanAccessOfferingAsync(ClaimsPrincipal user, int courseOfferingId, CancellationToken cancellationToken = default)
    {
        if (user.IsInRole("Admin") || user.IsInRole("SAdmin"))
            return true;

        if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return false;

        if (user.IsInRole("Doctor"))
            return await context.CourseOfferingDoctors.AnyAsync(x => x.CourseOfferingID == courseOfferingId && x.DoctorID == userId, cancellationToken);

        if (user.IsInRole("TeachingAssistant"))
            return await context.CourseOfferingTAs.AnyAsync(x => x.CourseOfferingID == courseOfferingId && x.TeacherAssistID == userId, cancellationToken);

        return false;
    }

    /// <summary>
    /// Can Manage Grade Async.
    /// </summary>
    public async Task<bool> CanManageGradeAsync(ClaimsPrincipal user, int enrollmentId, int moduleId, CancellationToken cancellationToken = default)
    {
        var offeringId = await context.Enrollments
            .Where(x => x.EnrollmentID == enrollmentId && x.CourseOffering.AssessmentModules.Any(m => m.ModuleID == moduleId))
            .Select(x => (int?)x.CourseOfferingID)
            .SingleOrDefaultAsync(cancellationToken);

        return offeringId.HasValue && await CanAccessOfferingAsync(user, offeringId.Value, cancellationToken);
    }
}
