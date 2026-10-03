using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// Read access to the append-only grade history (MED-06).
/// </summary>
/// <remarks>
/// The history exists so that "who changed this grade, from what, to what, and
/// when" can be answered after the fact. It is deliberately write-once: there is
/// no update or delete in this contract, because an audit trail that can be
/// rewritten is not an audit trail.
/// </remarks>
public interface ICourseGradeHistoryRepository
    : IGenericRepository<CourseGradeHistory>
{
    /// <summary>
    /// Gets the change trail of a single grade, newest first.
    /// </summary>
    /// <param name="gradeId">Grade whose history is requested.</param>
    /// <returns>The recorded changes, most recent first.</returns>
    Task<IEnumerable<CourseGradeHistory>> GetForGradeAsync(int gradeId);

    /// <summary>
    /// Gets the changes a single user made to a single grade, newest first.
    /// </summary>
    /// <param name="gradeId">Grade whose history is requested.</param>
    /// <param name="changedBy">Account whose entries are wanted.</param>
    /// <returns>The recorded changes, most recent first.</returns>
    Task<IEnumerable<CourseGradeHistory>> GetForGradeByUserAsync(
        int gradeId,
        int changedBy);
}