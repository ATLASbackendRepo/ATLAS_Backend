using ATLAsDCL.Data;
using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.InterFaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ATLAsDCL.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AtLASDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public IAssessmentModuleRepository AssessmentModules { get; }
    public ICourseGradeRepository CourseGrades { get; }
    public ICourseOfferingDoctorRepository CourseOfferingDoctors { get; }
    public ICourseOfferingRepository CourseOfferings { get; }
    public ICourseOfferingTARepository CourseOfferingTAs { get; }
    public ICourseRepository Courses { get; }
    public IDepartmentRepository Departments { get; }
    public ICourseGroupMessageRepository CourseGroupMessages { get; }
    public INotificationRepository Notifications { get; }
    public IEnrollmentRepository Enrollments { get; }
    public ISemesterRepository Semesters { get; }
    public IStudentRepository Students { get; }
    public IStaffProfileRepository StaffProfile { get; }
    public IStudentProfileRepository StudentProfile { get; }
    public IUserRepository Users { get; }

    public UnitOfWork(
        AtLASDbContext context,
        IAssessmentModuleRepository assessmentModuleRepository,
        ICourseGradeRepository courseGradeRepository,
        ICourseOfferingDoctorRepository courseOfferingDoctorRepository,
        ICourseOfferingRepository courseOfferingRepository,
        ICourseOfferingTARepository courseOfferingTARepository,
        ICourseRepository courseRepository,
        IDepartmentRepository departmentRepository,
        IEnrollmentRepository enrollmentRepository,
        ISemesterRepository semesterRepository,
        IStudentRepository studentRepository,
        IUserRepository userRepository,
        ICourseGroupMessageRepository courseGroupMessageRepository,
        INotificationRepository notificationRepository,
        IStudentProfileRepository studentProfileRepository,
        IStaffProfileRepository staffProfileRepository)
    {
        _context = context;

        AssessmentModules = assessmentModuleRepository;
        CourseGrades = courseGradeRepository;
        CourseOfferingDoctors = courseOfferingDoctorRepository;
        CourseOfferings = courseOfferingRepository;
        CourseOfferingTAs = courseOfferingTARepository;
        Courses = courseRepository;
        Departments = departmentRepository;
        Enrollments = enrollmentRepository;
        Semesters = semesterRepository;
        Students = studentRepository;
        Users = userRepository;
        CourseGroupMessages = courseGroupMessageRepository;
        Notifications = notificationRepository;
        StudentProfile = studentProfileRepository;
        StaffProfile = staffProfileRepository;
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            return;

        _currentTransaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }

    public IStaffProfileRepository staffProfile => StaffProfile;
}