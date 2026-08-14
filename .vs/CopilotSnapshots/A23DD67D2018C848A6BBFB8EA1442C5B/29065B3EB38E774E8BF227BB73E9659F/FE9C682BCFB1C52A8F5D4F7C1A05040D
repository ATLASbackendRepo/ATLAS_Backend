using ATLASDomain.Interfaces.Repositories;


namespace ATLAsDCL.UnitOfWork;

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

 

    IUserRepository Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}