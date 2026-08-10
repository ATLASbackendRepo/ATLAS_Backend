using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

public interface ICourseGradeRepository : IGenericRepository<CourseGrade>
{
    Task<IEnumerable<CourseGrade>> GetGradesByEnrollmentAsync(int enrollmentId);
    Task<CourseGrade?> GetGradeByEnrollmentAndModuleAsync(int enrollmentId, int moduleId);
    Task<double> GetAvgGradesByModuleEnrollmentAsync(int moduleId, int enrollmentId);
    Task<PagedResultDto<StudentCardDto>> GetCourseStudentsPagedAsync(GetStudentsPagedQueryModel param);

    Task<List<CourseGrade>> GetByEnrollmentAndModulesAsync(
        List<int> enrollmentIds,
        List<int> moduleIds);

    Task AddRangeAsync(List<CourseGrade> grades);

    void UpdateRange(List<CourseGrade> grades);
}
