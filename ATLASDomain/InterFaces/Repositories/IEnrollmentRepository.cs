using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.DTO.Enrollment;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;
using ATLASDomain.Views;

namespace ATLASDomain.Interfaces.Repositories;

public interface IEnrollmentRepository : IGenericRepository<Enrollment>
{
    Task<bool> IsStudentEnrolledAsync(int studentId, int courseOfferingId,DateTime Semester);

    Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId);

    Task<int?> GetEnrollmentIdAsync(string academicId, int courseOfferingId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(int courseOfferingId);
    Task<List<int?>> GetStudentUserIdsByCourseOfferingAsync(int courseOfferingId);

    Task<SemesterStudentRankingView?> GetStudentSemesterRankAsync(
      int studentId, int semesterId,  CancellationToken cancellationToken);

    Task<List<SemesterStudentRankingView>> GetTopRankedStudentsInSemesterAsync(
        int semesterId, int academicLevel, int topCount, CancellationToken cancellationToken);


    Task<int> BulkEnrollStudentsFastAsync(BulkCreateEnrollmentDTO dto);
    Task<bool> AddSingleStudentToEnrollmentAsync(string AcdimicID, BulkCreateEnrollmentDTO dto);
    Task<bool> DeleteEnrollmentByIdAsync(int enrollmentId);
    Task<PagedResultDto<CourseEnrollmentSummaryDto>> GetEnrollmentsOverviewAsync(
      string? searchKeyword = null,
      int? doctorId = null,
      int pageIndex = 1,
      int pageSize = 10);
}