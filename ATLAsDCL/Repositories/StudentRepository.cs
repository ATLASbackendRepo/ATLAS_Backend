using ATLAsDCL.Data;
using ATLASDomain.DTO.Student;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class StudentRepository
    : GenericRepository<Student>, IStudentRepository
{
    public StudentRepository(AtLASDbContext context)
        : base(context)
    {
    }

    public async Task<Student?> GetStudentWithDepartmentAsync(int studentId)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
    }

    public async Task<List<StudentDTO>> GetAllStudents(StudentFilterDTO fillter)
    {
        var students = _context.Students
               .AsNoTracking()
               .AsQueryable();

         if(!string.IsNullOrEmpty(fillter.Search))
         {
            students = students.Where(s => s.User.FullName.Contains(fillter.Search)
            || s.StudentCode.Contains(fillter.Search)|| s.AcademicId.Contains(fillter.Search));
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
                Departmentname = s.Department.D_Name ?? string.Empty,
                IsActive = s.IsActive
            })
            .ToListAsync();
        return studentDtos;
    }

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

    public async Task<Student?> GetStudentWithUserAsync(string studentCode)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentCode.Contains(studentCode));
    }
    public async Task<Student?> GetStudentByUserIDAsync(int UserID)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId  == UserID);
    }

    public async Task<bool> StudentCodeExistsAsync(string studentCode)
    {
        return await _context.Students
            .AnyAsync(s => s.StudentCode == studentCode);
    }


    public async Task<Student?> GetStudentByCodeAsync(string studentCode)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentCode == studentCode);
    }
    public async Task<Student?> GetStudentByAcademicIDAsync(string academicID)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.AcademicId == academicID);
    }

    public async Task<IEnumerable<Student>> GetAllStudentWithLevelAsync(int level)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.AcademicLevel == level)
            .ToListAsync();
    }
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
