using ATLAsDCL.Data;
using ATLAsDCL.Repositories;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.InterFaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ATLAsDCL.UnitOfWork;

/// <summary>
/// Unit Of Work.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    /// <summary>
    /// _context.
    /// </summary>
    private readonly AtLASDbContext _context;
    /// <summary>
    /// _current Transaction.
    /// </summary>
    private IDbContextTransaction? _currentTransaction;

    /// <summary>
    /// Assessment Modules.
    /// </summary>
    public IAssessmentModuleRepository AssessmentModules { get; }
    /// <summary>
    /// Course Grades.
    /// </summary>
    public ICourseGradeRepository CourseGrades { get; }
    /// <summary>
    /// Course Offering Doctors.
    /// </summary>
    public ICourseOfferingDoctorRepository CourseOfferingDoctors { get; }
    /// <summary>
    /// Course Offerings.
    /// </summary>
    public ICourseOfferingRepository CourseOfferings { get; }
    /// <summary>
    /// Course Offering TAs.
    /// </summary>
    public ICourseOfferingTARepository CourseOfferingTAs { get; }
    /// <summary>
    /// Courses.
    /// </summary>
    public ICourseRepository Courses { get; }
    /// <summary>
    /// Departments.
    /// </summary>
    public IDepartmentRepository Departments { get; }
    /// <summary>
    /// Course Group Messages.
    /// </summary>
    public ICourseGroupMessageRepository CourseGroupMessages { get; }

    /// <summary>
    /// Course Group Message Read.
    /// </summary>
    public ICourseGroupMessageReadRepository CourseGroupMessageRead { get; }
    /// <summary>
    /// Notifications.
    /// </summary>
    public INotificationRepository Notifications { get; }
    /// <summary>
    /// Enrollments.
    /// </summary>
    public IEnrollmentRepository Enrollments { get; }
    /// <summary>
    /// Semesters.
    /// </summary>
    public ISemesterRepository Semesters { get; }
    /// <summary>
    /// Students.
    /// </summary>
    public IStudentRepository Students { get; }
    /// <summary>
    /// Staff Profile.
    /// </summary>
    public IStaffProfileRepository StaffProfile { get; }
    /// <summary>
    /// Student Profile.
    /// </summary>
    public IStudentProfileRepository StudentProfile { get; }
    /// <summary>
    /// Users.
    /// </summary>
    public IUserRepository Users { get; }

 /// <summary>
 /// iinstitutions.
 /// </summary>
 public IinstitutionRepository iinstitutions { get; }

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
        IStaffProfileRepository staffProfileRepository,
        ICourseGroupMessageReadRepository courseGroupMessageRead
        , IGradeAppealRequestRepository gradeAppealRequestRepository
        , IinstitutionRepository iinstitution)
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
        CourseGroupMessageRead = courseGroupMessageRead;
        GradeAppealRequest = gradeAppealRequestRepository;
        iinstitutions = iinstitution;
    }

    /// <summary>
    /// Save Changes Async.
    /// </summary>
    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begin Transaction Async.
    /// </summary>
    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            return;

        _currentTransaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commit Transaction Async.
    /// </summary>
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

    /// <summary>
    /// Rollback Transaction Async.
    /// </summary>
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

    /// <summary>
    /// Dispose.
    /// </summary>
    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
    /// <summary>
    /// staff Profile.
    /// </summary>
    public IStaffProfileRepository staffProfile => StaffProfile;

    /// <summary>
    /// _course Group Messages Read.
    /// </summary>
    private ICourseGroupMessageReadRepository? _courseGroupMessagesRead;

    public ICourseGroupMessageReadRepository CourseGroupMessagesRead
    {
        get
        {
            if (_courseGroupMessagesRead == null)
            {
                _courseGroupMessagesRead =
                    new CourseGroupMessageReadRepository(_context);
            }

            return _courseGroupMessagesRead;
        }
    }
 


    /// <summary>
    /// Grade Appeal Request.
    /// </summary>
    public IGradeAppealRequestRepository GradeAppealRequest { get; }

    public IGradeAppealRequestRepository GradeAppealRequests
        => GradeAppealRequest;

    /// <summary>
    /// _institution.
    /// </summary>
    private IinstitutionRepository? _institution;
    public IinstitutionRepository Institutions
    {
        get
        {
            if (_institution == null)
            {
                _institution =
                    new institutionRepository(_context);
            }

            return _institution;
        }
    }
}
