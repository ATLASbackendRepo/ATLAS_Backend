using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

public class CourseOfferingDoctorRepository
    : GenericRepository<CourseOfferingDoctor>, ICourseOfferingDoctorRepository
{
    public CourseOfferingDoctorRepository(AtLASDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<CourseOfferingDoctor>> GetByDoctorAsync(int doctorId)
    {
        return await _context.CourseOfferingDoctors
            .AsNoTracking()
            .Include(c => c.CourseOffering)
            .Where(c => c.DoctorID == doctorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseOfferingDoctor>> GetDoctorsByOfferingAsync(int offeringId)
    {
        return await _context.CourseOfferingDoctors
            .AsNoTracking()
            .Include(c => c.CourseOffering)
            .Where(c => c.CourseOfferingID == offeringId)
            .ToListAsync();
    }
    public async Task<IEnumerable<int>> GetDoctorIdsByCourseOfferingIdAsync(int offeringId)
    {
        return await _context.CourseOfferingDoctors
            .AsNoTracking()
            .Include(c => c.CourseOffering)
            .Where(c => c.CourseOfferingID == offeringId)
            .Select(c => c.DoctorID)
            .ToListAsync();
    }
}
