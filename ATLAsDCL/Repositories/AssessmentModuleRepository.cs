using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

public class AssessmentModuleRepository
    : GenericRepository<AssessmentModule>, IAssessmentModuleRepository
{
    public AssessmentModuleRepository(AtLASDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<AssessmentModule>> GetModulesByCourseOfferingAsync(int courseOfferingId)
    {
        return await _context.AssessmentModules
            .AsNoTracking()
            .Where(a => a.CourseOfferingID == courseOfferingId)
            .OrderBy(a => a.ModuleID)
            .ToListAsync();
    }


}