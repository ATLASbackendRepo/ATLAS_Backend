using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

/// <summary>
/// ICourse Offering TARepository.
/// </summary>
public interface ICourseOfferingTARepository
    : IGenericRepository<CourseOfferingTA>
{
    Task<IEnumerable<CourseOfferingTA>> GetTAsByOfferingAsync(int offeringId);
}
