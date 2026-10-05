using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Semester Repository.
/// </summary>
public class SemesterRepository
    : GenericRepository<Semester>, ISemesterRepository
{
    public SemesterRepository(AtLASDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Is Grading Closed For Offering Async.
    /// Resolves the offering's semester in one query; a missing semester row is
    /// treated as open grading so the grade handlers keep working on data that
    /// predates the foreign key (MED-07).
    /// </summary>
    public async Task<bool> IsGradingClosedForOfferingAsync(int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _context.Semesters
            .AsNoTracking()
            .Where(s => s.CourseOfferings.Any(o => o.CourseOfferingID == courseOfferingId))
            .Select(s => s.IsGradingClosed)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
