using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// ICourse Grade Repository.
/// </summary>
public interface ICourseGradeRepository : IGenericRepository<CourseGrade>
{
    /// <summary>
    /// Gets every grade recorded for an enrollment.
    /// </summary>
    /// <param name="enrollmentId">Enrollment identifier.</param>
    /// <returns>The grades for that enrollment.</returns>
    Task<IEnumerable<CourseGrade>> GetGradesByEnrollmentAsync(int enrollmentId);

    /// <summary>
    /// Gets a student's grade for one assessment module.
    /// </summary>
    /// <param name="enrollmentId">Enrollment identifier.</param>
    /// <param name="moduleId">Assessment module identifier.</param>
    /// <returns>The grade, or null when none has been recorded.</returns>
    Task<CourseGrade?> GetGradeByEnrollmentAndModuleAsync(int enrollmentId, int moduleId);

    /// <summary>
    /// Gets the average grade a student scored across the modules of an offering.
    /// </summary>
    /// <param name="moduleId">Assessment module identifier.</param>
    /// <param name="enrollmentId">Enrollment identifier.</param>
    /// <returns>The average grade; 0 when the student has no grades.</returns>
    Task<double> GetAvgGradesByModuleEnrollmentAsync(int moduleId, int enrollmentId);

    /// <summary>
    /// Gets a page of the students on a course offering together with their grades.
    /// </summary>
    /// <param name="param">Paging, filtering and search criteria.</param>
    /// <returns>The requested page of student cards.</returns>
    Task<PagedResultDto<StudentCardDto>> GetCourseStudentsPagedAsync(GetStudentsPagedQueryModel param);

    /// <summary>
    /// Gets the export rows for a course offering.
    /// The student profile fields and per-module grades are resolved
    /// with explicit EF Core projections using a fixed number of queries.
    /// </summary>
    /// <param name="courseOfferingId">Course offering identifier.</param>
    /// <returns>Export rows containing student data and module grades.</returns>
    Task<List<CourseGradesExportDto>> GetCourseGradesForExportAsync(int courseOfferingId);

    /// <summary>
    /// Gets the existing grades for the given enrollments and modules in one
    /// query, so a bulk import can tell inserts from updates without a round
    /// trip per row.
    /// </summary>
    /// <param name="enrollmentIds">Enrollment identifiers to include.</param>
    /// <param name="moduleIds">Assessment module identifiers to include.</param>
    /// <returns>The matching grades.</returns>
    Task<List<CourseGrade>> GetByEnrollmentAndModulesAsync(
        List<int> enrollmentIds,
        List<int> moduleIds);

    /// <summary>
    /// Stages a batch of new grades for insertion.
    /// </summary>
    /// <param name="grades">The grades to add.</param>
    Task AddRangeAsync(List<CourseGrade> grades);

    /// <summary>
    /// Stages a batch of existing grades for update.
    /// </summary>
    /// <param name="grades">The grades to update.</param>
    void UpdateRange(List<CourseGrade> grades);
}
