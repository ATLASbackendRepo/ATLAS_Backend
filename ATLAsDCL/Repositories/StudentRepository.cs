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
    public async Task<Student?> GetStudentWithDepartmentAsync(int studentId)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
    }

    /// <summary>
    /// Get All Students.
    /// </summary>
    public async Task<List<StudentDTO>> GetAllStudents(StudentFilterDTO fillter)
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
            .ToListAsync();
        return studentDtos;
    }

    /// <summary>
    /// Get Student With Enrollments Async.
    /// </summary>
    public async Task<Student?> GetStudentWithEnrollmentsAsync(int studentId)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.CourseOffering)
                .ThenInclude(co => co.Course)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
    }

    /// <summary>
    /// Get Student With User Async.
    /// </summary>
    public async Task<Student?> GetStudentWithUserAsync(string studentCode)
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
            .FirstOrDefaultAsync(s => s.StudentCode == code);
    }
    /// <summary>
    /// Get Student By User IDAsync.
    /// </summary>
    public async Task<Student?> GetStudentByUserIDAsync(int UserID)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId  == UserID);
    }

    /// <summary>
    /// Student Code Exists Async.
    /// </summary>
    public async Task<bool> StudentCodeExistsAsync(string studentCode)
    {
        return await _context.Students
            .AnyAsync(s => s.StudentCode == studentCode);
    }


    /// <summary>
    /// Get Student By Code Async.
    /// </summary>
    public async Task<Student?> GetStudentByCodeAsync(string studentCode)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentCode == studentCode);
    }
    /// <summary>
    /// Get Student By Academic IDAsync.
    /// </summary>
    public async Task<Student?> GetStudentByAcademicIDAsync(string academicID)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.AcademicId == academicID);
    }

    /// <summary>
    /// Get All Student With Level Async.
    /// </summary>
    public async Task<IEnumerable<Student>> GetAllStudentWithLevelAsync(int level)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.AcademicLevel == level)
            .ToListAsync();
    }
    /// <summary>
    /// Get All Student With Courseffering Async.
    /// </summary>
    public async Task<IEnumerable<Student>> GetAllStudentWithCoursefferingAsync(int courseOfferingID)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.CourseOffering)
            .Where(s => s.Enrollments.Any(e => e.CourseOfferingID == courseOfferingID))
            .ToListAsync();
    }


    /// <summary>
    /// Get All Student With Mode Async.
    /// </summary>
    public async Task<IEnumerable<StudentGradeDto>> GetAllStudentWithModeAsync(int courseOfferingID, int modelId)
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
            .ToListAsync();
    }
    /// <summary>
    /// Get Student With Mode Async.
    /// </summary>
    public async Task<StudentGradeDto?> GetStudentWithModeAsync(int courseOfferingID, int modelId, string StudentCode)
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
            .FirstOrDefaultAsync(s => s.StudentCode == StudentCode);
    }
}
