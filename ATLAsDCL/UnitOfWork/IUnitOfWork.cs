using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.InterFaces.Repositories;


namespace ATLAsDCL.UnitOfWork;

/// <summary>
/// IUnit Of Work.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Assessment modules (graded components) that belong to a course offering.
    /// </summary>
    IAssessmentModuleRepository AssessmentModules { get; }

    /// <summary>
    /// Grades awarded to students for assessment modules.
    /// </summary>
    ICourseGradeRepository CourseGrades { get; }

    /// <summary>
    /// Doctors assigned to teach a course offering.
    /// </summary>
    ICourseOfferingDoctorRepository CourseOfferingDoctors { get; }

    /// <summary>
    /// Course offerings: a course taught in a semester.
    /// </summary>
    ICourseOfferingRepository CourseOfferings { get; }

    /// <summary>
    /// Teaching assistants assigned to a course offering.
    /// </summary>
    ICourseOfferingTARepository CourseOfferingTAs { get; }

    /// <summary>
    /// Course catalogue entries.
    /// </summary>
    ICourseRepository Courses { get; }

    /// <summary>
    /// Departments, each belonging to an institution.
    /// </summary>
    IDepartmentRepository Departments { get; }

    /// <summary>
    /// Student enrollments in course offerings.
    /// </summary>
    IEnrollmentRepository Enrollments { get; }

    /// <summary>
    /// Academic semesters.
    /// </summary>
    ISemesterRepository Semesters { get; }

    /// <summary>
    /// Student records.
    /// </summary>
    IStudentRepository Students { get; }

    /// <summary>
    /// Staff Profile. LOW-12: the lowercase staffProfile alias is gone; this is
    /// the single name, so callers cannot drift between two spellings.
    /// </summary>
    IStaffProfileRepository StaffProfile { get; }

    /// <summary>
    /// Student profile records.
    /// </summary>
    IStudentProfileRepository StudentProfile { get; }

    /// <summary>
    /// User accounts, including their credentials and security stamp.
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Messages posted to a course group chat.
    /// </summary>
    ICourseGroupMessageRepository CourseGroupMessages { get; }

    /// <summary>
    /// Per-user read state for course group chat messages.
    /// </summary>
    ICourseGroupMessageReadRepository CourseGroupMessagesRead { get; }

    /// <summary>
    /// In-app notifications delivered to users.
    /// </summary>
    INotificationRepository Notifications { get; }

    /// <summary>
    /// Grade appeal requests raised by students and answered by doctors.
    /// </summary>
    IGradeAppealRequestRepository GradeAppealRequests { get; }

    /// <summary>
    /// Append-only history of grade changes (MED-06).
    /// </summary>
    ICourseGradeHistoryRepository CourseGradeHistory { get; }

    /// <summary>
    /// Institutions (tenants) that own departments and staff.
    /// </summary>
    IinstitutionRepository Institutions { get; }

    /// <summary>
    /// Saves every pending change to the database in a single round trip.
    /// </summary>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>The number of rows written.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens an explicit database transaction. Use when several saves must
    /// succeed or fail together.
    /// </summary>
    /// <param name="cancellationToken">Cancels the operation.</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the transaction opened by <see cref="BeginTransactionAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancels the operation.</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction opened by <see cref="BeginTransactionAsync"/>,
    /// discarding any pending changes.
    /// </summary>
    /// <param name="cancellationToken">Cancels the operation.</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
