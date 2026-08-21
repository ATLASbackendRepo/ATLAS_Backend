using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

public interface ICourseOfferingDoctorRepository
    : IGenericRepository<CourseOfferingDoctor>
{
    Task<IEnumerable<CourseOfferingDoctor>> GetDoctorsByOfferingAsync(int offeringId);
    Task<IEnumerable<int>> GetDoctorIdsByCourseOfferingIdAsync(int offeringId);
}