using ATLAsDCL.Data;
using ATLASDomain.DTO.Course;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Course Repository.
/// </summary>
public class CourseRepository
    : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(AtLASDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Get All Course.
    /// </summary>
    public async Task<List<CourseDTO>> GetAllCourse(string? Name_Filter, int? institutionId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Courses
             .AsNoTracking()
             .AsQueryable();

        if (institutionId.HasValue)
        {
            query = query.Where(c => c.InstitutionID == institutionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(Name_Filter))
        {
            var searchTerm = $"%{Name_Filter}%";
            query = query.Where(c => EF.Functions.Like(c.C_Name, searchTerm));
        }

        var courses = await query
            .Select(C => new CourseDTO
            {
                Id = C.CourseID,
                CourseName = C.C_Name,
                CreditHours = C.Credit_Hours,
                IsPractical = C.IsPractical
            })
            .ToListAsync(cancellationToken);

        return courses;
    }
}
