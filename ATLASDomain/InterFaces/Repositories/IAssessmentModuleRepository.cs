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

    /// <summary>
    /// Gets the sum of <see cref="AssessmentModule.MaxDegree"/> over every module
    /// of the course offering, so the Create/Update handlers can keep the offering
    /// total within <see cref="AssessmentModule.MaximumOfferingTotalMaxDegree"/> (MED-07).
    /// </summary>
    /// <param name="courseOfferingId">Course offering identifier.</param>
    /// <returns>Sum of the module max degrees; 0 when the offering has no modules.</returns>
    Task<decimal> GetTotalMaxDegreeByCourseOfferingAsync(int courseOfferingId);

    /// <summary>
    /// Gets the highest degree already recorded in the module's grades, so the
    /// Update handler can refuse to lower <see cref="AssessmentModule.MaxDegree"/>
    /// below a grade that exists (MED-07).
    /// </summary>
    /// <param name="moduleId">Assessment module identifier.</param>
    /// <returns>Highest recorded degree; 0 when the module has no grades.</returns>
    Task<decimal> GetMaxDegreeByModuleAsync(int moduleId);
}

