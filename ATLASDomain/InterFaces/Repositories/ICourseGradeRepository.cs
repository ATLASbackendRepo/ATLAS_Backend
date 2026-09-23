using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// ICourse Grade Repository.
/// </summary>
public interface ICourseGradeRepository : IGenericRepository<CourseGrade>
{
    Task<IEnumerable<CourseGrade>> GetGradesByEnrollmentAsync(int enrollmentId);
    Task<CourseGrade?> GetGradeByEnrollmentAndModuleAsync(int enrollmentId, int moduleId);
    Task<double> GetAvgGradesByModuleEnrollmentAsync(int moduleId, int enrollmentId);
    Task<PagedResultDto<StudentCardDto>> GetCourseStudentsPagedAsync(GetStudentsPagedQueryModel param);

    /// <summary>
    /// Gets the export rows for a course offering.
    /// The student profile fields and per-module grades are resolved
    /// with explicit EF Core projections using a fixed number of queries.
    /// </summary>
    /// <param name="courseOfferingId">Course offering identifier.</param>
    /// <returns>Export rows containing student data and module grades.</returns>
    Task<List<CourseGradesExportDto>> GetCourseGradesForExportAsync(int courseOfferingId);

    Task<List<CourseGrade>> GetByEnrollmentAndModulesAsync(
        List<int> enrollmentIds,
        List<int> moduleIds);

    Task AddRangeAsync(List<CourseGrade> grades);

    void UpdateRange(List<CourseGrade> grades);
}
