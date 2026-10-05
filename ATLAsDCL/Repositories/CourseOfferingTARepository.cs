using ATLAsDCL.Data;
using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Course Offering TARepository.
/// </summary>
public class CourseOfferingTARepository
    : GenericRepository<CourseOfferingTA>, ICourseOfferingTARepository
{
    public CourseOfferingTARepository(AtLASDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Get By TAAsync.
    /// </summary>
    public async Task<IEnumerable<CourseOfferingTA>> GetByTAAsync(int taId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseOfferingTAs
            .AsNoTracking()
            .Include(c => c.CourseOffering)
            .Where(c => c.TeacherAssistID == taId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get TAs By Offering Async.
    /// </summary>
    public async Task<IEnumerable<CourseOfferingTA>> GetTAsByOfferingAsync(int offeringId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseOfferingTAs
            .AsNoTracking()
            .Include(c => c.CourseOffering)
            .Where(c => c.CourseOfferingID == offeringId)
            .ToListAsync(cancellationToken);
    }
}   
