using ATLAS.Claims;
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
    /// <remarks>
    /// MED-16: this used to return true for any Admin, so one institution's admin
    /// could read and modify another institution's offerings. Admins are now
    /// tenant-scoped: an Admin may only reach an offering belonging to the
    /// institution named in their token. SAdmin is the single global role and is
    /// deliberately left unscoped, because managing the institutions themselves is
    /// exactly what it is for.
    /// </remarks>
    public async Task<bool> CanAccessOfferingAsync(ClaimsPrincipal user, int courseOfferingId, CancellationToken cancellationToken = default)
    {
        // SAdmin is the only global role. It is trusted across every tenant.
        if (user.IsInRole("SAdmin"))
            return true;

        if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return false;

        if (user.IsInRole("Doctor"))
            return await context.CourseOfferingDoctors.AnyAsync(x => x.CourseOfferingID == courseOfferingId && x.DoctorID == userId, cancellationToken);

        if (user.IsInRole("TeachingAssistant"))
            return await context.CourseOfferingTAs.AnyAsync(x => x.CourseOfferingID == courseOfferingId && x.TeacherAssistID == userId, cancellationToken);

        if (user.IsInRole("Admin"))
        {
            if (!TryGetInstitutionId(user, out var adminInstitutionId))
            {
                return false;
            }

            var offeringInstitutionId = await context.CourseOfferings
                .Where(co => co.CourseOfferingID == courseOfferingId)
                .Select(co => co.InstitutionID)
                .FirstOrDefaultAsync(cancellationToken);

            if (offeringInstitutionId.HasValue)
            {
                return offeringInstitutionId.Value == adminInstitutionId;
            }

            var enrolledInstitutionIds = await context.Enrollments
                .Where(e => e.CourseOfferingID == courseOfferingId)
                .Select(e => e.Student.InstitutionID)
                .Distinct()
                .ToListAsync(cancellationToken);

            return enrolledInstitutionIds.Count > 0 &&
                   enrolledInstitutionIds.All(id => id == adminInstitutionId);
        }

        return false;
    }

    /// <summary>
    /// Reads the institution the authenticated account belongs to from its token.
    /// </summary>
    /// <param name="user">Authenticated principal.</param>
    /// <param name="institutionId">Receives the institution id when present.</param>
    /// <returns>True when the token carries a usable institution id.</returns>
    private static bool TryGetInstitutionId(ClaimsPrincipal user, out int institutionId)
    {
        var raw = user.FindFirstValue(AtlasClaims.InstitutionId);
        return int.TryParse(raw, out institutionId) && institutionId > 0;
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
