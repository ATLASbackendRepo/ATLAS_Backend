using ATLAsDCL.Data;
using ATLASDomain.DTO.Student;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<Student>> GetAllStudents()
    {
        var students = await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .ToListAsync();
        return students;
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

    public async Task<Student?> GetStudentWithUserAsync(int studentId)
    {
        return await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
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
    public async Task<StudentGradeDto> GetStudentWithModeAsync(int courseOfferingID, int modelId, string StudentCode)
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
