using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

/// <summary>
/// ICourse Offering Doctor Repository.
/// </summary>
public interface ICourseOfferingDoctorRepository
    : IGenericRepository<CourseOfferingDoctor>
{
    /// <summary>
    /// Gets the doctor user ids assigned to a course offering.
    /// </summary>
    /// <param name="offeringId">Course offering identifier.</param>
    /// <returns>The assigned doctor user ids.</returns>
    Task<IEnumerable<int>> GetDoctorsByOfferingAsync(int offeringId);

    /// <summary>
    /// Gets the doctor user ids of a course offering. Equivalent to
    /// <see cref="GetDoctorsByOfferingAsync"/> and kept for callers that name the
    /// course offering explicitly.
    /// </summary>
    /// <param name="offeringId">Course offering identifier.</param>
    /// <returns>The assigned doctor user ids.</returns>
    Task<IEnumerable<int>> GetDoctorIdsByCourseOfferingIdAsync(int offeringId);
}
