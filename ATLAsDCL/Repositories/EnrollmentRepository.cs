using ATLAsDCL.Data;
using ATLAsDCL.Repositories;
using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.DTO.Enrollment;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
/// Enrollment Repository.
/// </summary>
public class EnrollmentRepository
    : GenericRepository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(AtLASDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Get Enrollment Details Async.
    /// </summary>
    public async Task<Enrollment?> GetEnrollmentDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.CourseOffering)
            .FirstOrDefaultAsync(e => e.EnrollmentID == id, cancellationToken);
    }
    /// <summary>
    /// Get Enrollment Id Async.
    /// </summary>
    public async Task<int?> GetEnrollmentIdAsync(string academicId, int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.Student.AcademicId == academicId && e.CourseOfferingID == courseOfferingId)
            .Select(e => (int?)e.EnrollmentID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get Enrollment Ids By Academic Numbers Async.
    /// Resolves many academic numbers with a single batched query so that
    /// bulk imports do not perform one database round-trip per row.
    /// </summary>
    /// <param name="academicNumbers">academic Numbers.</param>
    /// <param name="courseOfferingId">course Offering Id.</param>
    /// <param name="cancellationToken">cancellation Token.</param>
    public async Task<Dictionary<string, int>> GetEnrollmentIdsByAcademicNumbersAsync(
        List<string> academicNumbers,
        int courseOfferingId,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);

        if (academicNumbers == null || academicNumbers.Count == 0)
            return result;

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Where(e =>
                e.CourseOfferingID == courseOfferingId &&
                academicNumbers.Contains(e.Student!.AcademicId!))
            .ToListAsync(cancellationToken);

        foreach (var enrollment in enrollments)
        {
            var academicId = enrollment.Student?.AcademicId;

            if (academicId != null)
                result[academicId] = enrollment.EnrollmentID;
        }

        return result;
    }

    /// <summary>
    /// Is Student Enrolled Async.
    /// </summary>
    public async Task<bool> IsStudentEnrolledAsync(int studentId, int courseOfferingId, DateTime semester, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
    .AsNoTracking()
    .AnyAsync(e =>
        e.StudentID == studentId &&
        e.CourseOfferingID == courseOfferingId &&
        e.CourseOffering.Semester.AcademicYear == semester.Date.Year.ToString(), cancellationToken);
    }

    /// <summary>
    /// Get Student Enrollments Async.
    /// </summary>
    public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.CourseOffering)
            .ThenInclude(co => co.Course)
            .Where(e => e.StudentID == studentId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get Course Enrollments Async.
    /// </summary>
    public async Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Where(e => e.CourseOfferingID == courseOfferingId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get Student Semester Rank Async.
    /// </summary>
    public async Task<SemesterStudentRankingView?> GetStudentSemesterRankAsync(
      int studentId, int semesterId, CancellationToken cancellationToken)
    {
        var isStudentActive = await _context.Students
            .AnyAsync(s => s.StudentId == studentId && s.IsActive, cancellationToken);

        if (!isStudentActive)
        {
            return null;
        }

        return await _context.SemesterStudentRankings
            .AsNoTracking()
            .Where(r => r.SemesterID == semesterId && r.StudentID == studentId )
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get Top Ranked Students In Semester Async.
    /// </summary>
    public async Task<List<SemesterStudentRankingView>> GetTopRankedStudentsInSemesterAsync(
        int semesterId, int AcademicLevel, int topCount, CancellationToken cancellationToken)
    {
        var activeStudentIds = _context.Students
            .Where(s => s.IsActive)
            .Select(s => s.StudentId);

        return await _context.SemesterStudentRankings
            .AsNoTracking()
            .Where(r => r.SemesterID == semesterId &&
                        r.AcademicLevel == AcademicLevel &&
                        r.Rank <= topCount &&
                        activeStudentIds.Contains(r.StudentID))
            .OrderBy(r => r.Rank)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get Student User Ids By Course Offering Async.
    /// </summary>
    public async Task<List<int?>> GetStudentUserIdsByCourseOfferingAsync(int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Where(e => e.CourseOfferingID == courseOfferingId)
            .Select(e => e.Student.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Bulk Enroll Students Fast Async.
    /// Enrolls every active student of one department and academic level into an
    /// existing course offering with a single INSERT ... SELECT statement.
    /// The semester and the course offering must already exist: enrolling never
    /// creates them as a side effect. The acting user is stored on every row so
    /// the audit trail is complete.
    /// </summary>
    /// <param name="dto">Course, semester, department and level to enroll.</param>
    /// <param name="createdByUserId">User that performs the enrollment.</param>
    /// <param name="institutionId">Optional institution scope to restrict lookups and enrolled students.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of inserted enrollment rows.</returns>
    public async Task<int> BulkEnrollStudentsFastAsync(
        BulkCreateEnrollmentDTO dto,
        int createdByUserId,
        int? institutionId = null,
        CancellationToken cancellationToken = default)
    {
        if (createdByUserId <= 0)
        {
            throw new ArgumentException(
                "The acting user is required to enroll students.");
        }

        var semesterQuery = _context.Semesters.AsNoTracking();
        if (institutionId.HasValue)
        {
            semesterQuery = semesterQuery.Where(s => s.InstitutionID == institutionId.Value);
        }

        var semester = await semesterQuery
            .FirstOrDefaultAsync(s => s.AcademicYear == dto.AcademicYear
                                   && s.SemesterName == dto.SemesterName, cancellationToken);

        if (semester is null)
        {
            throw new KeyNotFoundException(
                $"Semester '{dto.SemesterName}' of academic year " +
                $"'{dto.AcademicYear}' was not found.");
        }

        var courseOfferingQuery = _context.CourseOfferings.AsNoTracking();
        if (institutionId.HasValue)
        {
            courseOfferingQuery = courseOfferingQuery.Where(co => co.InstitutionID == institutionId.Value);
        }

        var courseOffering = await courseOfferingQuery
            .Where(co => co.CourseID == dto.CourseID
                      && co.SemesterID == semester.SemesterID)
            .FirstOrDefaultAsync(cancellationToken);

        if (courseOffering is null)
        {
            throw new KeyNotFoundException(
                $"Course {dto.CourseID} is not offered in semester " +
                $"'{dto.SemesterName}' of academic year '{dto.AcademicYear}'.");
        }

        if (courseOffering.InstitutionID != semester.InstitutionID)
        {
            throw new ArgumentException("The course offering and semester belong to different institutions.");
        }

        var courseOfferingId = courseOffering.CourseOfferingID;
        var targetInstId = institutionId ?? courseOffering.InstitutionID;

        if (!targetInstId.HasValue)
        {
            throw new InvalidOperationException("Course offering does not have an assigned institution.");
        }

        var studentQuery = _context.Students
            .Where(s => s.DepartmentId == dto.DepartmentID
                     && s.AcademicLevel == dto.level
                     && s.IsActive
                     && s.InstitutionID == targetInstId.Value);

        var studentsToEnroll = await studentQuery
            .Where(s => !_context.Enrollments.Any(e => e.StudentID == s.StudentId && e.CourseOfferingID == courseOfferingId))
            .Select(s => s.StudentId)
            .ToListAsync(cancellationToken);

        if (studentsToEnroll.Count == 0)
            return 0;

        var now = DateTime.UtcNow;
        var newEnrollments = studentsToEnroll.Select(sid => new Enrollment
        {
            StudentID = sid,
            CourseOfferingID = courseOfferingId,
            EnrollmentDate = now,
            CreatedByUserID = createdByUserId
        }).ToList();

        await _context.Enrollments.AddRangeAsync(newEnrollments, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Add Single Student To Enrollment Async.
    /// The semester and the course offering must already exist and the student
    /// is resolved by academic id, so nothing is created as a side effect.
    /// </summary>
    /// <param name="AcdimicID">Academic id of the student to enroll.</param>
    /// <param name="dto">Course and semester the student is enrolled into.</param>
    /// <param name="createdByUserId">User that performs the enrollment.</param>
    /// <param name="institutionId">Optional institution scope to restrict lookups and enrolled student.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when a new enrollment row was inserted.</returns>
    public async Task<bool> AddSingleStudentToEnrollmentAsync(
        string AcdimicID,
        BulkCreateEnrollmentDTO dto,
        int createdByUserId,
        int? institutionId = null,
        CancellationToken cancellationToken = default)
    {
        if (createdByUserId <= 0)
        {
            throw new ArgumentException(
                "The acting user is required to enroll a student.");
        }

        var semesterQuery = _context.Semesters.AsNoTracking();
        if (institutionId.HasValue)
        {
            semesterQuery = semesterQuery.Where(s => s.InstitutionID == institutionId.Value);
        }

        var semester = await semesterQuery
            .FirstOrDefaultAsync(s => s.AcademicYear == dto.AcademicYear
                                   && s.SemesterName == dto.SemesterName, cancellationToken);

        if (semester is null)
        {
            throw new KeyNotFoundException(
                $"Semester '{dto.SemesterName}' of academic year " +
                $"'{dto.AcademicYear}' was not found.");
        }

        var courseOfferingQuery = _context.CourseOfferings.AsNoTracking();
        if (institutionId.HasValue)
        {
            courseOfferingQuery = courseOfferingQuery.Where(co => co.InstitutionID == institutionId.Value);
        }

        var courseOffering = await courseOfferingQuery
            .Where(co => co.CourseID == dto.CourseID
                      && co.SemesterID == semester.SemesterID)
            .FirstOrDefaultAsync(cancellationToken);

        if (courseOffering is null)
        {
            throw new KeyNotFoundException(
                $"Course {dto.CourseID} is not offered in semester " +
                $"'{dto.SemesterName}' of academic year '{dto.AcademicYear}'.");
        }

        if (courseOffering.InstitutionID != semester.InstitutionID)
        {
            throw new ArgumentException("The course offering and semester belong to different institutions.");
        }

        var studentQuery = _context.Students.AsNoTracking();
        if (institutionId.HasValue)
        {
            studentQuery = studentQuery.Where(s => s.InstitutionID == institutionId.Value);
        }

        var student = await studentQuery
            .Where(s => s.AcademicId == AcdimicID)
            .FirstOrDefaultAsync(cancellationToken);

        if (student is null)
        {
            throw new KeyNotFoundException(
                $"Student with academic ID '{AcdimicID}' was not found.");
        }

        if (courseOffering.InstitutionID != student.InstitutionID)
        {
            throw new ArgumentException("The student and course offering belong to different institutions.");
        }

        var isAlreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentID == student.StudentId
                        && e.CourseOfferingID == courseOffering.CourseOfferingID, cancellationToken);

        if (isAlreadyEnrolled)
        {
            return false;
        }

        _context.Enrollments.Add(new Enrollment
        {
            StudentID = student.StudentId,
            CourseOfferingID = courseOffering.CourseOfferingID,
            EnrollmentDate = DateTime.UtcNow,
            CreatedByUserID = createdByUserId
        });

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Delete Enrollment By Id Async with institution check.
    /// </summary>
    public async Task<bool> DeleteEnrollmentByIdAsync(int enrollmentId, int? institutionId = null, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.CourseOffering)
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.EnrollmentID == enrollmentId, cancellationToken);

        if (enrollment is null)
        {
            return false;
        }

        if (institutionId.HasValue)
        {
            var offeringInstId = enrollment.CourseOffering?.InstitutionID;
            var studentInstId = enrollment.Student?.InstitutionID;

            if (offeringInstId != institutionId.Value || studentInstId != institutionId.Value)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to delete enrollments outside your institution.");
            }
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

  /// <summary>
  /// Get Enrollments Overview Async.
  /// </summary>
  public async Task<PagedResultDto<CourseEnrollmentSummaryDto>> GetEnrollmentsOverviewAsync(
      string? searchKeyword = null,
      int? doctorId = null,
      int pageIndex = 1,
      int pageSize = 10,
      int? institutionId = null,
      CancellationToken cancellationToken = default)
  {
   
      var query = _context.CourseOfferings.AsNoTracking();

      if (institutionId.HasValue)
      {
          query = query.Where(co => co.InstitutionID == institutionId.Value);
      }
      
      if (!string.IsNullOrWhiteSpace(searchKeyword))
      {
          var search = searchKeyword.Trim().ToLower();
          query = query.Where(co => co.Course.C_Name.ToLower().Contains(search));
      }

      if (doctorId.HasValue && doctorId.Value > 0)
      {
          query = query.Where(co => co.CourseOfferingDoctors.Any(cd => cd.DoctorID == doctorId.Value));
      }

     
      var totalCount = await query.CountAsync(cancellationToken);


        var items = await query
           .OrderByDescending(co => co.CourseOfferingID)
           .Skip((pageIndex - 1) * pageSize)
           .Take(pageSize)
           .Select(co => new CourseEnrollmentSummaryDto
           {
               CourseOfferingID = co.CourseOfferingID,
               CourseID = co.CourseID,
               CourseName = co.Course.C_Name,
               Hours = co.Course.Credit_Hours,

              
               Doctors = co.CourseOfferingDoctors
                   .Select(cd => cd.Doctor.FullName)
                   .ToList(),

               TeachingAssistants = co.CourseOfferingTAs
                   .Select(cta => cta.TeachingAssistant.FullName)
                   .ToList(),

               
               StudentsCount = co.Enrollments.Count(),
               EnrollmentIDs = co.Enrollments.Select(e => e.EnrollmentID).ToList()
           })
           .ToListAsync(cancellationToken);

        return new PagedResultDto<CourseEnrollmentSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageIndex,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets student IDs of students already enrolled in a course offering.
    /// </summary>
    public async Task<HashSet<int>> GetEnrolledStudentIdsAsync(
        int courseOfferingId,
        IEnumerable<int> studentIds,
        CancellationToken cancellationToken = default)
    {
        var ids = studentIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new HashSet<int>();
        }

        var enrolled = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseOfferingID == courseOfferingId && ids.Contains(e.StudentID))
            .Select(e => e.StudentID)
            .ToListAsync(cancellationToken);

        return new HashSet<int>(enrolled);
    }
}
