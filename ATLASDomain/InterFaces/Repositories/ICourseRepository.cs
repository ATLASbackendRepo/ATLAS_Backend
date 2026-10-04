using ATLASDomain.DTO.Course;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// ICourse Repository.
/// </summary>
public interface ICourseRepository : IGenericRepository<Course>
{
    // need CRUD operations for Course entity only now (generic)

    /// <summary>
    /// Gets the course catalogue, optionally filtered by a name fragment.
    /// </summary>
    /// <param name="Name_Filter">
    /// Substring to match against the course name; null or empty returns everything.
    /// </param>
    /// <returns>The matching courses.</returns>
    Task<List<CourseDTO>> GetAllCourse(string? Name_Filter);

}
