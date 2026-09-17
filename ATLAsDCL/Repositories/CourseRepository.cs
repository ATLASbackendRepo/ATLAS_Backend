using ATLAsDCL.Data;
using ATLASDomain.DTO.Course;
using ATLASDomain.DTO.Department;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

public class CourseRepository
    : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(AtLASDbContext context)
        : base(context)
    {


    }

    public async Task<List<CourseDTO>> GetAllCourse(string? Name_Filter)
    {
        var query = _context.Courses
             .AsNoTracking()
             .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Name_Filter))
        {
            var searchTerm = $"%{Name_Filter}%";
            query = query.Where(c => EF.Functions.Like(c.C_Name, searchTerm));
        }


        var Courses = await query
       .Select(C => new CourseDTO
       {
           Id = C.CourseID,
           CourseName =C.C_Name,
           CreditHours = C.Credit_Hours,
           IsPractical = C.IsPractical
          
       })
            .ToListAsync();

        return Courses;
    }

    //public async Task<bool> CourseCodeExistsAsync(string code)
    //{
    //    return await _context.Courses
    //        .AnyAsync(c => c.CourseCode == code);
    //}
}