using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

/// <summary>
/// ICourse Offering TARepository.
/// </summary>
public interface ICourseOfferingTARepository
    : IGenericRepository<CourseOfferingTA>
{
    /// <summary>
    /// Gets the teaching assistant assignments of a course offering.
    /// </summary>
    /// <param name="offeringId">Course offering identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The teaching assistant assignments.</returns>
    Task<IEnumerable<CourseOfferingTA>> GetTAsByOfferingAsync(int offeringId, CancellationToken cancellationToken = default);
}
