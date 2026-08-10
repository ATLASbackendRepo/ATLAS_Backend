using ATLAsDCL.Data;
using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

public class CourseGradeRepository
    : GenericRepository<CourseGrade>, ICourseGradeRepository
{
    public CourseGradeRepository(AtLASDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<CourseGrade>> GetGradesByEnrollmentAsync(int enrollmentId)
    {
        return await _context.CourseGrades
            .AsNoTracking()
            .Where(g => g.EnrollmentID == enrollmentId)
            .Select(g => new CourseGrade
            {
                CG_ID = g.CG_ID,
                EnrollmentID = g.EnrollmentID,
                ModuleID = g.ModuleID,
                StudentDegree = g.StudentDegree
            })
            .ToListAsync();
    }


    public async Task<CourseGrade?> GetGradeByEnrollmentAndModuleAsync(int enrollmentId, int moduleId)
    {
        return await _context.CourseGrades
            .FirstOrDefaultAsync(g => g.EnrollmentID == enrollmentId && g.ModuleID == moduleId);
    }

    public async Task<double> GetAvgGradesByModuleEnrollmentAsync(int moduleId, int enrollmentId)
    {
        return await _context.CourseGrades
            .Where(g => g.ModuleID == moduleId && g.EnrollmentID == enrollmentId)
            .Select(x => (double?)x.StudentDegree)
             .AverageAsync() ?? 0;
    }




        public async Task<PagedResultDto<StudentCardDto>> GetCourseStudentsPagedAsync(GetStudentsPagedQueryModel param)
    {
        var pageNumber = Math.Max(1, param.PageNumber);
        var pageSize = Math.Clamp(param.PageSize, 1, GetStudentsPagedQueryModel.MaxPageSize);

        var query = _context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseOfferingID == param.CourseOfferingId);

        
        if (!string.IsNullOrWhiteSpace(param.SearchTerm))
        {
            var term = param.SearchTerm.Trim().ToLower();
            query = query.Where(e => e.Student.User.FullName.ToLower().Contains(term)
                                  || e.Student.AcademicId.Contains(term)
                                  || e.Student.StudentCode.ToLower().Contains(term));
        }

        
        var totalCount = await query.CountAsync();

      
        var items = await query
            .OrderBy(e => e.Student.User.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new StudentCardDto
            {
                StudentId = e.StudentID,
                StudentName = e.Student.User.FullName,
                StudentCode = e.Student.StudentCode,
                AcademicId = e.Student.AcademicId,

                EnrollmentId = e.EnrollmentID,

                Grades = _context.AssessmentModules
                    .Where(m => m.CourseOfferingID == param.CourseOfferingId)
                    .Select(m => new StudentModuleGradeDto
                    {
                        ModuleName = m.ModuleName,
                        MaxDegree = (double)m.MaxDegree,
                        StudentDegree = _context.CourseGrades
                            .Where(g => g.EnrollmentID == e.EnrollmentID && g.ModuleID == m.ModuleID)
                            .Select(g => (double?)g.StudentDegree)
                            .FirstOrDefault()
                    }).ToList()
            })
            .ToListAsync();

        
        return new PagedResultDto<StudentCardDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }


    public async Task<List<CourseGrade>> GetByEnrollmentAndModulesAsync(
    List<int> enrollmentIds,
    List<int> moduleIds)
    {
        return await _context.CourseGrades
            .Where(x => enrollmentIds.Contains(x.EnrollmentID)
                     && moduleIds.Contains(x.ModuleID))
            .ToListAsync();
    }
    public async Task AddRangeAsync(List<CourseGrade> grades)
    {
        await _context.CourseGrades.AddRangeAsync(grades);
    }

    public void UpdateRange(List<CourseGrade> grades)
    {
        _context.CourseGrades.UpdateRange(grades);
    }


}
