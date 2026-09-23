using ATLAsDCL.Data;
using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Course Grade Repository.
/// </summary>
public class CourseGradeRepository
    : GenericRepository<CourseGrade>, ICourseGradeRepository
{
    public CourseGradeRepository(AtLASDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Get Grades By Enrollment Async.
    /// </summary>
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


    /// <summary>
    /// Get Grade By Enrollment And Module Async.
    /// </summary>
    public async Task<CourseGrade?> GetGradeByEnrollmentAndModuleAsync(int enrollmentId, int moduleId)
    {
        return await _context.CourseGrades
            .FirstOrDefaultAsync(g => g.EnrollmentID == enrollmentId && g.ModuleID == moduleId);
    }

    /// <summary>
    /// Get Avg Grades By Module Enrollment Async.
    /// </summary>
    public async Task<double> GetAvgGradesByModuleEnrollmentAsync(int moduleId, int enrollmentId)
    {
        return await _context.CourseGrades
            .Where(g => g.ModuleID == moduleId && g.EnrollmentID == enrollmentId)
            .Select(x => (double?)x.StudentDegree)
             .AverageAsync() ?? 0;
    }




        /// <summary>
        /// Get Course Students Paged Async.
        /// </summary>
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

            query = query.Where(e =>
                e.Student!.User!.FullName!.ToLower().Contains(term)
                || e.Student!.AcademicId!.Contains(term)
                || e.Student!.StudentCode!.ToLower().Contains(term));
        }


        var totalCount = await query.CountAsync();


        var items = await query
           .OrderBy(e => e.Student!.User!.FullName)
           .Skip((pageNumber - 1) * pageSize)
           .Take(pageSize)
           .Select(e => new StudentCardDto
           {
               StudentId = e.StudentID,

               StudentName = e.Student!.User!.FullName ?? string.Empty,

               StudentCode = e.Student!.StudentCode ?? string.Empty,

               AcademicId = e.Student!.AcademicId ?? string.Empty,

               EnrollmentId = e.EnrollmentID,

               Grades = _context.AssessmentModules
                   .Where(m => m.CourseOfferingID == param.CourseOfferingId)
                   .Select(m => new StudentModuleGradeDto
                   {
                       ModuleName = m.ModuleName,
                       MaxDegree = (double)m.MaxDegree,

                       StudentDegree = _context.CourseGrades
                           .Where(g =>
                               g.EnrollmentID == e.EnrollmentID &&
                               g.ModuleID == m.ModuleID)
                           .Select(g => (double?)g.StudentDegree)
                           .FirstOrDefault()
                   })
                   .ToList()
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
    public async Task<List<CourseGradesExportDto>>
    GetCourseGradesForExportAsync(int courseOfferingId)
    {
        // 1) Enrollments + student profile data via explicit projection.
        var rows = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseOfferingID == courseOfferingId)
            .OrderBy(e => e.Student!.User!.FullName)
            .Select(e => new CourseGradesExportDto
            {
                StudentId = e.StudentID,
                StudentName = e.Student!.User!.FullName ?? string.Empty,
                StudentCode = e.Student!.StudentCode ?? string.Empty,
                AcademicId = e.Student!.AcademicId ?? string.Empty,
                EnrollmentId = e.EnrollmentID
            })
            .ToListAsync();

        if (rows.Count == 0)
            return rows;

        // 2) Assessment modules once (header/ordering data).
        var modules = await _context.AssessmentModules
            .AsNoTracking()
            .Where(m => m.CourseOfferingID == courseOfferingId)
            .OrderBy(m => m.ModuleID)
            .ToListAsync();

        var enrollmentIds = rows.Select(x => x.EnrollmentId).ToList();
        var moduleIds = modules.Select(m => m.ModuleID).ToList();

        // 3) All grades for the offering in a single query.
        var grades = await _context.CourseGrades
            .AsNoTracking()
            .Where(g =>
                enrollmentIds.Contains(g.EnrollmentID) &&
                moduleIds.Contains(g.ModuleID))
            .ToListAsync();

        var gradesByKey = grades.ToDictionary(
            g => (g.EnrollmentID, g.ModuleID),
            g => (double?)g.StudentDegree);

        // 4) Compose the projection into the final DTOs in memory.
        var result = new List<CourseGradesExportDto>(rows.Count);

        foreach (var row in rows)
        {
            var student = new CourseGradesExportDto
            {
                StudentId = row.StudentId,
                StudentName = row.StudentName,
                StudentCode = row.StudentCode,
                AcademicId = row.AcademicId,
                EnrollmentId = row.EnrollmentId,
                Grades = new List<EXportStudentModuleGradeDto>()
            };

            foreach (var module in modules)
            {
                double? degree = null;
                gradesByKey.TryGetValue(
                    (row.EnrollmentId, module.ModuleID),
                    out degree);

                student.Grades.Add(new EXportStudentModuleGradeDto
                {
                    ModuleId = module.ModuleID,
                    ModuleName = module.ModuleName,
                    MaxDegree = (double)module.MaxDegree,
                    StudentDegree = degree
                });
            }

            result.Add(student);
        }

        return result;
    }

    /// <summary>
    /// Get By Enrollment And Modules Async.
    /// </summary>
    public async Task<List<CourseGrade>> GetByEnrollmentAndModulesAsync(
    List<int> enrollmentIds,
    List<int> moduleIds)
    {
        return await _context.CourseGrades
            .Where(x => enrollmentIds.Contains(x.EnrollmentID)
                     && moduleIds.Contains(x.ModuleID))
            .ToListAsync();
    }
    /// <summary>
    /// Add Range Async.
    /// </summary>
    public async Task AddRangeAsync(List<CourseGrade> grades)
    {
        await _context.CourseGrades.AddRangeAsync(grades);
    }

    /// <summary>
    /// Update Range.
    /// </summary>
    public void UpdateRange(List<CourseGrade> grades)
    {
        _context.CourseGrades.UpdateRange(grades);
    }


}
