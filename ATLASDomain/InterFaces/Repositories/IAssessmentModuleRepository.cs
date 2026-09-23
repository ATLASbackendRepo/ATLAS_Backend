using ATLASDomain.DTO.AssessmentModule;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// IAssessment Module Repository.
/// </summary>
public interface IAssessmentModuleRepository : IGenericRepository<AssessmentModule>
{
    Task<IEnumerable<AssessmentModelDisplayDTO>> GetModulesByCourseOfferingAsync(int courseOfferingId);
}

