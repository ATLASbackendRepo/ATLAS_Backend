using ATLAsDCL.Data;
using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;

public class CourseOfferingTARepository
    : GenericRepository<CourseOfferingTA>, ICourseOfferingTARepository
{
    public CourseOfferingTARepository(AtLASDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<CourseOfferingTA>> GetByTAAsync(int taId)
    {
        return await _context.CourseOfferingTAs
            .AsNoTracking()
            .Include(c => c.CourseOffering)
            .Where(c => c.TAID == taId)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseOfferingTA>> GetTAsByOfferingAsync(int offeringId)
    {
        return await _context.CourseOfferingTAs
            .AsNoTracking()
            .Include(c => c.CourseOffering)
            .Where(c => c.CourseOfferingID == offeringId)
            .ToListAsync();
    }
}   
