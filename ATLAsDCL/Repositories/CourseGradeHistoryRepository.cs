using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ATLAsDCL.Repositories;

/// <summary>
/// Course Grade History Repository (MED-06).
/// </summary>
public class CourseGradeHistoryRepository
    : GenericRepository<CourseGradeHistory>, ICourseGradeHistoryRepository
{
    public CourseGradeHistoryRepository(AtLASDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Get For Grade Async.
    /// </summary>
    public async Task<IEnumerable<CourseGradeHistory>> GetForGradeAsync(int gradeId, CancellationToken cancellationToken = default)
    {
        // AsNoTracking: the history is a read-only record, so there is nothing to
        // write back and tracking it would only add change-tracking overhead.
        return await _context.Set<CourseGradeHistory>()
            .AsNoTracking()
            .Where(h => h.CG_ID == gradeId)
            .OrderByDescending(h => h.ChangedAtUtc)
            .ThenByDescending(h => h.HistoryId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get For Grade By User Async.
    /// </summary>
    public async Task<IEnumerable<CourseGradeHistory>> GetForGradeByUserAsync(
        int gradeId,
        int changedBy, CancellationToken cancellationToken = default)
    {
        return await _context.Set<CourseGradeHistory>()
            .AsNoTracking()
            .Where(h => h.CG_ID == gradeId && h.ChangedBy == changedBy)
            .OrderByDescending(h => h.ChangedAtUtc)
            .ThenByDescending(h => h.HistoryId)
            .ToListAsync(cancellationToken);
    }
}