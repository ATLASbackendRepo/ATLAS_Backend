using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

public interface IAssessmentModuleRepository : IGenericRepository<AssessmentModule>
{
    Task<IEnumerable<AssessmentModule>> GetModulesByCourseOfferingAsync(int courseOfferingId);
}
