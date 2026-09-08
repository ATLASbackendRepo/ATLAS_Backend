using ATLASDomain.DTO.AssessmentModule;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

public interface IAssessmentModuleRepository : IGenericRepository<AssessmentModule>
{
    Task<IEnumerable<AssessmentModelDisplayDTO>> GetModulesByCourseOfferingAsync(int courseOfferingId);
}

