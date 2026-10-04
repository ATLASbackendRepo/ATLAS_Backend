using ATLAsDCL.Data;
using ATLASDomain.DTO.AssessmentModule;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Assessment Module Repository.
/// </summary>
public class AssessmentModuleRepository
    : GenericRepository<AssessmentModule>, IAssessmentModuleRepository
{
    public AssessmentModuleRepository(AtLASDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Get Modules By Course Offering Async.
    /// </summary>
    public async Task<IEnumerable<AssessmentModelDisplayDTO>> GetModulesByCourseOfferingAsync(int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _context.AssessmentModules
            .AsNoTracking()
            .Where(a => a.CourseOfferingID == courseOfferingId)
            .Select(a => new AssessmentModelDisplayDTO
            {
                ModuleId = a.ModuleID,
                ModuleName = a.ModuleName,
                MaxDegree = a.MaxDegree
            })
            .OrderBy(a => a.ModuleId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get Total Max Degree By Course Offering Async.
    /// Sums the module max degrees of one offering so the handlers can enforce
    /// the per-offering total cap (MED-07).
    /// </summary>
    public async Task<decimal> GetTotalMaxDegreeByCourseOfferingAsync(int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _context.AssessmentModules
            .AsNoTracking()
            .Where(a => a.CourseOfferingID == courseOfferingId)
            .SumAsync(a => (decimal?)a.MaxDegree, cancellationToken) ?? 0m;
    }

    /// <summary>
    /// Get Max Degree By Module Async.
    /// Returns the highest degree already recorded in the module's grades, or 0
    /// when no grade exists, so MaxDegree cannot be lowered below a recorded
    /// grade (MED-07).
    /// </summary>
    public async Task<decimal> GetMaxDegreeByModuleAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseGrades
            .AsNoTracking()
            .Where(g => g.ModuleID == moduleId)
            .MaxAsync(g => (decimal?)g.StudentDegree, cancellationToken) ?? 0m;
    }


}
