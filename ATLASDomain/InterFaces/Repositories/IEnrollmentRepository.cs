using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.DTO.Enrollment;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;
using ATLASDomain.Views;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// IEnrollment Repository.
/// </summary>
public interface IEnrollmentRepository : IGenericRepository<Enrollment>
{
    /// <summary>
    /// Checks whether a student is already enrolled in a course offering.
    /// </summary>
    /// <param name="studentId">Student identifier.</param>
    /// <param name="courseOfferingId">Course offering identifier.</param>
    /// <param name="Semester">Semester the offering belongs to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the enrollment already exists.</returns>
    Task<bool> IsStudentEnrolledAsync(int studentId, int courseOfferingId,DateTime Semester, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets every enrollment of a student.
    /// </summary>
    /// <param name="studentId">Student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The student's enrollments.</returns>
    Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the enrollment id of a student in a course offering.
    /// </summary>
    /// <param name="academicId">Student academic number.</param>
    /// <param name="courseOfferingId">Course offering identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The enrollment id, or null when the student is not enrolled.</returns>
    Task<int?> GetEnrollmentIdAsync(string academicId, int courseOfferingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds enrollment identifiers for many academic numbers of one course offering
    /// using a single batched database query.
    /// </summary>
    /// <param name="academicNumbers">Distinct academic numbers to look up.</param>
    /// <param name="courseOfferingId">Course offering identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A map of academic number to matching enrollment identifier.
    /// Academic numbers without an enrollment are absent from the map.
    /// </returns>
    Task<Dictionary<string, int>> GetEnrollmentIdsByAcademicNumbersAsync(
        List<string> academicNumbers,
        int courseOfferingId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets every enrollment of a course offering.
    /// </summary>
    /// <param name="courseOfferingId">Course offering identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The enrollments of that offering.</returns>
    Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(int courseOfferingId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the user ids of every student enrolled in a course offering.
    /// </summary>
    /// <param name="courseOfferingId">Course offering identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The enrolled students' user ids.</returns>
    Task<List<int?>> GetStudentUserIdsByCourseOfferingAsync(int courseOfferingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a student's rank within a semester.
    /// </summary>
    /// <param name="studentId">Student identifier.</param>
    /// <param name="semesterId">Semester identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ranking row, or null when the student has no ranked grades.</returns>
    Task<SemesterStudentRankingView?> GetStudentSemesterRankAsync(
      int studentId, int semesterId,  CancellationToken cancellationToken);

    /// <summary>
    /// Gets the highest ranked students of a semester for one academic level.
    /// </summary>
    /// <param name="semesterId">Semester identifier.</param>
    /// <param name="academicLevel">Academic level to rank within.</param>
    /// <param name="topCount">Maximum number of students to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ranking rows, best first.</returns>
    Task<List<SemesterStudentRankingView>> GetTopRankedStudentsInSemesterAsync(
        int semesterId, int academicLevel, int topCount, CancellationToken cancellationToken);


    /// <summary>
    /// Enrolls every student matching the filter in the requested offerings using
    /// set-based statements rather than one round trip per student.
    /// </summary>
    /// <param name="dto">The filter and the offerings to enroll into.</param>
    /// <param name="createdByUserId">User performing the import, recorded for audit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of enrollments created.</returns>
    Task<int> BulkEnrollStudentsFastAsync(BulkCreateEnrollmentDTO dto, int createdByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enrolls a single student, identified by academic number, in the requested
    /// offerings.
    /// </summary>
    /// <param name="AcdimicID">Student academic number.</param>
    /// <param name="dto">The offerings to enroll into.</param>
    /// <param name="createdByUserId">User performing the enrollment, recorded for audit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the enrollment was created.</returns>
    Task<bool> AddSingleStudentToEnrollmentAsync(string AcdimicID, BulkCreateEnrollmentDTO dto, int createdByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an enrollment.
    /// </summary>
    /// <param name="enrollmentId">Enrollment identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the enrollment was found and deleted.</returns>
    Task<bool> DeleteEnrollmentByIdAsync(int enrollmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a page of enrollments summarised per course offering, for the
    /// administration overview screen.
    /// </summary>
    /// <param name="searchKeyword">Optional free-text filter.</param>
    /// <param name="doctorId">Optional doctor to filter on.</param>
    /// <param name="pageIndex">One-based page number.</param>
    /// <param name="pageSize">Number of rows per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested page of enrollment summaries.</returns>
    Task<PagedResultDto<CourseEnrollmentSummaryDto>> GetEnrollmentsOverviewAsync(
      string? searchKeyword = null,
      int? doctorId = null,
      int pageIndex = 1,
      int pageSize = 10, CancellationToken cancellationToken = default);
}
