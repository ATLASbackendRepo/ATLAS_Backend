using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

/// <summary>
/// ICourse Offering Doctor Repository.
/// </summary>
public interface ICourseOfferingDoctorRepository
    : IGenericRepository<CourseOfferingDoctor>
{
    Task<IEnumerable<int>> GetDoctorsByOfferingAsync(int offeringId);
    Task<IEnumerable<int>> GetDoctorIdsByCourseOfferingIdAsync(int offeringId);
}
