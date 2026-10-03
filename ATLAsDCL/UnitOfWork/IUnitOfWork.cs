using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.InterFaces.Repositories;


namespace ATLAsDCL.UnitOfWork;

/// <summary>
/// IUnit Of Work.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IAssessmentModuleRepository AssessmentModules { get; }

    ICourseGradeRepository CourseGrades { get; }

    ICourseOfferingDoctorRepository CourseOfferingDoctors { get; }

    ICourseOfferingRepository CourseOfferings { get; }

    ICourseOfferingTARepository CourseOfferingTAs { get; }

    ICourseRepository Courses { get; }

    IDepartmentRepository Departments { get; }

    

    IEnrollmentRepository Enrollments { get; }

    ISemesterRepository Semesters { get; }

    IStudentRepository Students { get; }

 
    IStaffProfileRepository staffProfile { get; }

    IStudentProfileRepository StudentProfile { get; }
    IUserRepository Users { get; }
    ICourseGroupMessageRepository CourseGroupMessages { get; }
    ICourseGroupMessageReadRepository CourseGroupMessagesRead { get; }
    INotificationRepository Notifications { get; }

    IGradeAppealRequestRepository GradeAppealRequests { get; }

    /// <summary>
    /// Append-only history of grade changes (MED-06).
    /// </summary>
    ICourseGradeHistoryRepository CourseGradeHistory { get; }

    IinstitutionRepository Institutions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
