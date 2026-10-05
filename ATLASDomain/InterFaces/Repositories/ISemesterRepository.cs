using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// ISemester Repository.
/// </summary>
public interface ISemesterRepository : IGenericRepository<Semester>
{
    /// <summary>
    /// Gets whether the semester that owns the course offering has grading
    /// closed, so both grade-assignment handlers can refuse writes after the
    /// closing (MED-07).
    /// </summary>
    /// <param name="courseOfferingId">Course offering identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// True when the offering's semester exists and its grading is closed;
    /// false when grading is open or the semester row is missing.
    /// </returns>
    Task<bool> IsGradingClosedForOfferingAsync(int courseOfferingId, CancellationToken cancellationToken = default);
}
