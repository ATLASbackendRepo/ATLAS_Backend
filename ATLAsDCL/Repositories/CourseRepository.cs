using ATLAsDCL.Data;
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

    //public async Task<bool> CourseCodeExistsAsync(string code)
    //{
    //    return await _context.Courses
    //        .AnyAsync(c => c.CourseCode == code);
    //}
}