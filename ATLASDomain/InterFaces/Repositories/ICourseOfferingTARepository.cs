using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

public interface ICourseOfferingTARepository
    : IGenericRepository<CourseOfferingTA>
{
    Task<IEnumerable<CourseOfferingTA>> GetTAsByOfferingAsync(int offeringId);
}