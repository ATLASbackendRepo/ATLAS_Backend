using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;
using ATLASDomain.Views;

namespace ATLASDomain.Interfaces.Repositories;

public interface IEnrollmentRepository : IGenericRepository<Enrollment>
{
    Task<bool> IsStudentEnrolledAsync(int studentId, int courseOfferingId,DateTime Semester);

    Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId);

    Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(int courseOfferingId);
    Task<List<int?>> GetStudentUserIdsByCourseOfferingAsync(int courseOfferingId);

    Task<SemesterStudentRankingView?> GetStudentSemesterRankAsync(
      int studentId, int semesterId,  CancellationToken cancellationToken);

    Task<List<SemesterStudentRankingView>> GetTopRankedStudentsInSemesterAsync(
        int semesterId, int academicLevel, int topCount, CancellationToken cancellationToken);
}