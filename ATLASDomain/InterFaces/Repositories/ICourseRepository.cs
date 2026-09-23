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

    Task<List<CourseDTO>> GetAllCourse(string? Name_Filter);

}
