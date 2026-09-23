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
    /// </summary>
    public async Task<int> BulkEnrollStudentsFastAsync(BulkCreateEnrollmentDTO dto)
    {
        var semester = await _context.Semesters
        .FirstOrDefaultAsync(s => s.AcademicYear == dto.AcademicYear
                               && s.SemesterName == dto.SemesterName);

        if (semester == null)
        {
            semester = new Semester
            {
                AcademicYear = dto.AcademicYear,
                SemesterName = dto.SemesterName
            };

            _context.Semesters.Add(semester);
            await _context.SaveChangesAsync(); 
        }

       
        var courseOfferingId = await _context.CourseOfferings
            .Where(co => co.CourseID == dto.CourseID && co.SemesterID == semester.SemesterID)
            .Select(co => co.CourseOfferingID)
            .FirstOrDefaultAsync();

        if (courseOfferingId == 0)
        {
            var newOffering = new CourseOffering
            {
                CourseID = dto.CourseID,
                SemesterID = semester.SemesterID
            };

            _context.CourseOfferings.Add(newOffering);
            await _context.SaveChangesAsync();

            courseOfferingId = newOffering.CourseOfferingID;
        }

        var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync($@"
        INSERT INTO Enrollments (StudentID, CourseOfferingID, EnrollmentDate)
        SELECT 
            s.StudentID, 
            co.CourseOfferingID, 
            {DateTime.UtcNow}
        FROM Students s
        CROSS JOIN CourseOfferings co
        INNER JOIN Semesters sem ON co.SemesterID = sem.SemesterID
        WHERE s.DepartmentID = {dto.DepartmentID} 
          AND s.AcademicLevel = {dto.level} 
          AND s.IsActive = 1
          AND co.CourseID = {dto.CourseID}
          AND sem.AcademicYear = {dto.AcademicYear}
          AND sem.SemesterName = {dto.SemesterName}
          AND NOT EXISTS (
              SELECT 1 
              FROM Enrollments e 
              WHERE e.StudentID = s.StudentID 
                AND e.CourseOfferingID = co.CourseOfferingID
          );
    ");

        return rowsAffected; 
    }
    /// <summary>
    /// Add Single Student To Enrollment Async.
    /// </summary>
    public async Task<bool> AddSingleStudentToEnrollmentAsync(string AcdimicID, BulkCreateEnrollmentDTO dto)
    {


        
        var semester = await _context.Semesters
            .FirstOrDefaultAsync(s => s.AcademicYear == dto.AcademicYear
                                   && s.SemesterName == dto.SemesterName);

        if (semester == null)
        {
            semester = new Semester { AcademicYear = dto.AcademicYear, SemesterName = dto.SemesterName };
            _context.Semesters.Add(semester);
            await _context.SaveChangesAsync();
        }

       
        var courseOffering = await _context.CourseOfferings
            .FirstOrDefaultAsync(co => co.CourseID == dto.CourseID && co.SemesterID == semester.SemesterID);

        if (courseOffering == null)
        {
            courseOffering = new CourseOffering { CourseID = dto.CourseID, SemesterID = semester.SemesterID };
            _context.CourseOfferings.Add(courseOffering);
            await _context.SaveChangesAsync();
        }

        var StudentID = await _context.Students
                        .Where(s => s.AcademicId == AcdimicID)
                        .Select(s => s.StudentId)
                        .FirstOrDefaultAsync();
        
        var isAlreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentID == StudentID && e.CourseOfferingID == courseOffering.CourseOfferingID);

        if (isAlreadyEnrolled) return false; 

        
        var enrollment = new Enrollment
        {
            StudentID = StudentID,
            CourseOfferingID = courseOffering.CourseOfferingID,
            EnrollmentDate = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
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
