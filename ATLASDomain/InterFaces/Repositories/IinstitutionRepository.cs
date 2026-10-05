using ATLASDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.InterFaces.Repositories
{
    /// <summary>
    /// Iinstitution Repository.
    /// </summary>
    public interface IinstitutionRepository : IGenericRepository<Institution>
    {

        /// <summary>
        /// Looks up an institution by name.
        /// </summary>
        /// <param name="institutionName">Institution name to match.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The institution id, or 0 when no such institution exists.</returns>
        Task<int> GetInstitutionWithNameAsync(string institutionName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets every institution.
        /// </summary>
        /// <returns>All institutions.</returns>
        Task<List<InstitutionDto>> GetAllInstitutionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single institution by its identifier.
        /// </summary>
        /// <param name="id">Institution identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The institution, or null when it does not exist.</returns>
        Task<InstitutionDto> GetInstitutionByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new institution.
        /// </summary>
        /// <param name="dto">The institution to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created institution, including its new identifier.</returns>
        Task<InstitutionDto> CreateInstitutionAsync(CreateInstitutionDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renames an existing institution.
        /// </summary>
        /// <param name="dto">The institution to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True when the institution was found and updated.</returns>
        Task<bool> UpdateInstitutionAsync(UpdateInstitutionDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an institution.
        /// </summary>
        /// <param name="id">Institution identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True when the institution was found and deleted.</returns>
        Task<bool> DeleteInstitutionAsync(int id, CancellationToken cancellationToken = default);


    }
}
