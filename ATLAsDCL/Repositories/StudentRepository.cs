using ATLAsDCL.Data;
using ATLASDomain.DTO.Student;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

/// <summary>
/// Student Repository.
/// </summary>
public class StudentRepository
    : GenericRepository<Student>, IStudentRepository
{
    public StudentRepository(AtLASDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Get Student With Department Async.
    /// </summary>
    public async Task<Student?> GetStudentWithDepartmentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);
    }

    /// <summary>
    /// Get All Students.
    /// </summary>
    public async Task<List<StudentDTO>> GetAllStudents(StudentFilterDTO fillter, CancellationToken cancellationToken = default)
    {
        var students = _context.Students
               .AsNoTracking()
               .AsQueryable();

         if(!string.IsNullOrEmpty(fillter.Search))
         {
            students = students.Where(s =>
                (s.User != null && s.User.FullName.Contains(fillter.Search))
                || s.StudentCode.Contains(fillter.Search)
                || (s.AcademicId != null && s.AcademicId.Contains(fillter.Search)));
         }

         if(fillter.DepartmentId.HasValue)
         {
            students = students.Where(s => s.DepartmentId == fillter.DepartmentId.Value);
         }
         if(fillter.Level.HasValue)
         {
            students = students.Where(s => s.AcademicLevel == fillter.Level.Value);
         }
         if(fillter.Status.HasValue)
         {
            students = students.Where(s => s.IsActive == fillter.Status.Value);
         }
         if(fillter.InstitutionId.HasValue)
         {
            students = students.Where(s => s.InstitutionID == fillter.InstitutionId.Value);
         }


        // MED-12: this list is now bounded. Without the Skip/Take below the query
        // materialised the entire Students table, so one request could return every
        // row the system holds. The page size is clamped by PagedFilter.Normalize,
        // so a client cannot ask for an unbounded page by sending a huge number.
        var (pageNumber, pageSize) = fillter.Normalize();

         var studentDtos = await students
            .Select(s => new StudentDTO
            {
                StudentCode = s.StudentCode,
                StudentName = s.User.FullName ?? string.Empty,
                AcademicId = s.AcademicId??string.Empty,
                AcademicLevel = s.AcademicLevel,
                Departmentname = s.Department != null ? s.Department.D_Name ?? string.Empty : string.Empty,
                IsActive = s.IsActive
            })
            // A stable sort is required for paging to be correct: without a
            // deterministic order, rows can repeat or vanish between pages.
            .OrderBy(s => s.StudentCode)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return studentDtos;
    }

    /// <summary>
    /// Get Student With Enrollments Async.
    /// </summary>
    public async Task<Student?> GetStudentWithEnrollmentsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.CourseOffering)
                .ThenInclude(co => co.Course)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);
    }

    /// <summary>
    /// Get Student With User Async.
    /// </summary>
    public async Task<Student?> GetStudentWithUserAsync(string studentCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentCode))
        {
            return null;
        }

        var code = studentCode.Trim();

        // The write handlers mutate the returned graph, so the entity must stay
        // tracked, and the code must match exactly: a partial code must never
        // resolve to an arbitrary student.
        return await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentCode == code, cancellationToken);
    }
    /// <summary>
    /// Get Student By User IDAsync.
    /// </summary>
    public async Task<Student?> GetStudentByUserIDAsync(int UserID, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId  == UserID, cancellationToken);
    }

    /// <summary>
    /// Student Code Exists Async.
    /// </summary>
    public async Task<bool> StudentCodeExistsAsync(string studentCode, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AnyAsync(s => s.StudentCode == studentCode, cancellationToken);
    }


    /// <summary>
    /// Get Student By Code Async.
    /// </summary>
    public async Task<Student?> GetStudentByCodeAsync(string studentCode, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentCode == studentCode, cancellationToken);
    }
    /// <summary>
    /// Get Student By Academic IDAsync.
    /// </summary>
    public async Task<Student?> GetStudentByAcademicIDAsync(string academicID, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.AcademicId == academicID, cancellationToken);
    }

    /// <summary>
    /// Get All Student With Level Async.
    /// </summary>
    public async Task<IEnumerable<Student>> GetAllStudentWithLevelAsync(int level, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.AcademicLevel == level)
            .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get All Student With Courseffering Async.
    /// </summary>
    public async Task<IEnumerable<Student>> GetAllStudentWithCoursefferingAsync(int courseOfferingID, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.CourseOffering)
            .Where(s => s.Enrollments.Any(e => e.CourseOfferingID == courseOfferingID))
            .ToListAsync(cancellationToken);
    }


    /// <summary>
    /// Get All Student With Mode Async.
    /// </summary>
    public async Task<IEnumerable<StudentGradeDto>> GetAllStudentWithModeAsync(int courseOfferingID, int modelId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentGradeModels
            .AsNoTracking()
            .Where(s => s.CourseOfferingID == courseOfferingID && s.ModuleID == modelId)
            .Select(s => new StudentGradeDto
            {
                StudentCode = s.StudentCode,
                AcademicId = s.AcademicID,
                Studentname = s.FullName,
                Studentdegree = s.StudentDegree
            })
            .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get Student With Mode Async.
    /// </summary>
    public async Task<StudentGradeDto?> GetStudentWithModeAsync(int courseOfferingID, int modelId, string StudentCode, CancellationToken cancellationToken = default)
    {
        return await _context.StudentGradeModels
            .AsNoTracking()
         .Where(s => s.CourseOfferingID == courseOfferingID && s.ModuleID == modelId)
            .Select(s => new StudentGradeDto
            {
                StudentCode = s.StudentCode,
                AcademicId = s.AcademicID,
                Studentname = s.FullName,
                Studentdegree = s.StudentDegree
            })
            .FirstOrDefaultAsync(s => s.StudentCode == StudentCode, cancellationToken);
    }

    /// <summary>
    /// Gets matching students by their academic IDs.
    /// </summary>
    public async Task<List<Student>> GetStudentsByAcademicIdsAsync(
        IEnumerable<string> academicIds,
        CancellationToken cancellationToken = default)
    {
        var ids = academicIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (ids.Count == 0)
        {
            return new List<Student>();
        }

        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => (s.AcademicId != null && ids.Contains(s.AcademicId))
                     || (s.User != null && s.User.AcademicId != null && ids.Contains(s.User.AcademicId)))
            .ToListAsync(cancellationToken);
    }
}
