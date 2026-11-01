using ATLASDomain.DTO.Course;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// ICourse Repository.
/// </summary>
public interface ICourseRepository : IGenericRepository<Course>
{
    /// <summary>
    /// Gets the course catalogue, optionally filtered by a name fragment and institution ID.
    /// </summary>
    /// <param name="Name_Filter">Substring to match against the course name; null or empty returns everything.</param>
    /// <param name="institutionId">Institution ID to scope the results; null returns across institutions for SAdmin.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching courses.</returns>
    Task<List<CourseDTO>> GetAllCourse(string? Name_Filter, int? institutionId = null, CancellationToken cancellationToken = default);
}
