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
    public async Task<Enrollment?> GetEnrollmentDetailsAsync(int id)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.CourseOffering)
            .FirstOrDefaultAsync(e => e.EnrollmentID == id);
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
            .ToListAsync();

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
    public async Task<bool> IsStudentEnrolledAsync(int studentId, int courseOfferingId, DateTime semester)
    {
        return await _context.Enrollments
    .AsNoTracking()
    .AnyAsync(e =>
        e.StudentID == studentId &&
        e.CourseOfferingID == courseOfferingId &&
        e.CourseOffering.Semester.AcademicYear == semester.Date.Year.ToString());
    }

    /// <summary>
    /// Get Student Enrollments Async.
    /// </summary>
    public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.CourseOffering)
            .ThenInclude(co => co.Course)
            .Where(e => e.StudentID == studentId)
            .ToListAsync();
    }

    /// <summary>
    /// Get Course Enrollments Async.
    /// </summary>
    public async Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(int courseOfferingId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Where(e => e.CourseOfferingID == courseOfferingId)
            .ToListAsync();
    }

    /// <summary>
    /// Get Student Semester Rank Async.
    /// </summary>
    public async Task<SemesterStudentRankingView?> GetStudentSemesterRankAsync(
      int studentId, int semesterId, CancellationToken cancellationToken)
    {
        return await _context.SemesterStudentRankings
            .AsNoTracking()
            .Where(r => r.SemesterID == semesterId && r.StudentID == studentId )
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get Top Ranked Students In Semester Async.
    /// </summary>
    public async Task<List<SemesterStudentRankingView>> GetTopRankedStudentsInSemesterAsync(
        int semesterId,int AcademicLevel ,int topCount, CancellationToken cancellationToken)
    {
        return await _context.SemesterStudentRankings
            .AsNoTracking()
            .Where(r => r.SemesterID == semesterId && r.AcademicLevel == AcademicLevel && r.Rank <= topCount)
            .OrderBy(r => r.Rank)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get Student User Ids By Course Offering Async.
    /// </summary>
    public async Task<List<int?>> GetStudentUserIdsByCourseOfferingAsync(int courseOfferingId)
    {
        return await _context.Enrollments
            .Where(e => e.CourseOfferingID == courseOfferingId)
            .Select(e => e.Student.UserId)
            .Distinct()
            .ToListAsync();
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
    /// <returns>Number of inserted enrollment rows.</returns>
    public async Task<int> BulkEnrollStudentsFastAsync(
        BulkCreateEnrollmentDTO dto,
        int createdByUserId)
    {
        if (createdByUserId <= 0)
        {
            throw new ArgumentException(
                "The acting user is required to enroll students.");
        }

        var semester = await _context.Semesters
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.AcademicYear == dto.AcademicYear
                                   && s.SemesterName == dto.SemesterName);

        if (semester is null)
        {
            throw new KeyNotFoundException(
                $"Semester '{dto.SemesterName}' of academic year " +
                $"'{dto.AcademicYear}' was not found.");
        }

        var courseOfferingId = await _context.CourseOfferings
            .AsNoTracking()
            .Where(co => co.CourseID == dto.CourseID
                      && co.SemesterID == semester.SemesterID)
            .Select(co => co.CourseOfferingID)
            .FirstOrDefaultAsync();

        if (courseOfferingId == 0)
        {
            throw new KeyNotFoundException(
                $"Course {dto.CourseID} is not offered in semester " +
                $"'{dto.SemesterName}' of academic year '{dto.AcademicYear}'.");
        }

        var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync($@"
        INSERT INTO Enrollments (StudentID, CourseOfferingID, EnrollmentDate, CreatedByUserID)
        SELECT 
            s.StudentID, 
            {courseOfferingId}, 
            {DateTime.UtcNow},
            {createdByUserId}
        FROM Students s
        WHERE s.DepartmentID = {dto.DepartmentID} 
          AND s.AcademicLevel = {dto.level} 
          AND s.IsActive = 1
          AND NOT EXISTS (
              SELECT 1 
              FROM Enrollments e 
              WHERE e.StudentID = s.StudentID 
                AND e.CourseOfferingID = {courseOfferingId}
          );
    ");

        return rowsAffected;
    }

    /// <summary>
    /// Add Single Student To Enrollment Async.
    /// The semester and the course offering must already exist and the student
    /// is resolved by academic id, so nothing is created as a side effect.
    /// </summary>
    /// <param name="AcadimicID">Academic id of the student to enroll.</param>
    /// <param name="dto">Course and semester the student is enrolled into.</param>
    /// <param name="createdByUserId">User that performs the enrollment.</param>
    /// <returns>True when a new enrollment row was inserted.</returns>
    public async Task<bool> AddSingleStudentToEnrollmentAsync(
        string AcdimicID,
        BulkCreateEnrollmentDTO dto,
        int createdByUserId)
    {
        if (createdByUserId <= 0)
        {
            throw new ArgumentException(
                "The acting user is required to enroll a student.");
        }

        var semester = await _context.Semesters
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.AcademicYear == dto.AcademicYear
                                   && s.SemesterName == dto.SemesterName);

        if (semester is null)
        {
            throw new KeyNotFoundException(
                $"Semester '{dto.SemesterName}' of academic year " +
                $"'{dto.AcademicYear}' was not found.");
        }

        var courseOfferingId = await _context.CourseOfferings
            .AsNoTracking()
            .Where(co => co.CourseID == dto.CourseID
                      && co.SemesterID == semester.SemesterID)
            .Select(co => co.CourseOfferingID)
            .FirstOrDefaultAsync();

        if (courseOfferingId == 0)
        {
            throw new KeyNotFoundException(
                $"Course {dto.CourseID} is not offered in semester " +
                $"'{dto.SemesterName}' of academic year '{dto.AcademicYear}'.");
        }

        var studentId = await _context.Students
            .AsNoTracking()
            .Where(s => s.AcademicId == AcdimicID)
            .Select(s => s.StudentId)
            .FirstOrDefaultAsync();

        if (studentId == 0)
        {
            throw new KeyNotFoundException(
                $"Student with academic ID '{AcdimicID}' was not found.");
        }

        var isAlreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentID == studentId
                        && e.CourseOfferingID == courseOfferingId);

        if (isAlreadyEnrolled)
        {
            return false;
        }

        _context.Enrollments.Add(new Enrollment
        {
            StudentID = studentId,
            CourseOfferingID = courseOfferingId,
            EnrollmentDate = DateTime.UtcNow,
            CreatedByUserID = createdByUserId
        });

        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Delete Enrollment By Id Async.
    /// </summary>
    public async Task<bool> DeleteEnrollmentByIdAsync(int enrollmentId)
    {
        var deletedRows = await _context.Enrollments
            .Where(e => e.EnrollmentID == enrollmentId)
            .ExecuteDeleteAsync();

        return deletedRows > 0;
    }

  /// <summary>
  /// Get Enrollments Overview Async.
  /// </summary>
  public async Task<PagedResultDto<CourseEnrollmentSummaryDto>> GetEnrollmentsOverviewAsync(
      string? searchKeyword = null,
      int? doctorId = null,
      int pageIndex = 1,
      int pageSize = 10)
  {
   
      var query = _context.CourseOfferings.AsNoTracking();

      
      if (!string.IsNullOrWhiteSpace(searchKeyword))
      {
          var search = searchKeyword.Trim().ToLower();
          query = query.Where(co => co.Course.C_Name.ToLower().Contains(search));
      }

      if (doctorId.HasValue && doctorId.Value > 0)
      {
          query = query.Where(co => co.CourseOfferingDoctors.Any(cd => cd.DoctorID == doctorId.Value));
      }

     
      var totalCount = await query.CountAsync();


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
           .ToListAsync();

        return new PagedResultDto<CourseEnrollmentSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageIndex,
            PageSize = pageSize
        };
    }

    
}
