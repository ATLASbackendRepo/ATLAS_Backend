using ATLAsDCL.Data;
using ATLASDomain.DTO.AssessmentModule;
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

    public async Task<IEnumerable<AssessmentModelDisplayDTO>> GetModulesByCourseOfferingAsync(int courseOfferingId)
    {
        return await _context.AssessmentModules
            .AsNoTracking()
            .Where(a => a.CourseOfferingID == courseOfferingId)
            .Select(a => new AssessmentModelDisplayDTO
            {
                ModuleId = a.ModuleID,
                ModuleName = a.ModuleName,
                MaxDegree = a.MaxDegree
            })
            .OrderBy(a => a.ModuleId)
            .ToListAsync();
    }


}