using ATLAS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.InterFaces.Repositories
{
    /// <summary>
    /// IStudent Profile Repository.
    /// </summary>
    public interface IStudentProfileRepository :IGenericRepository<StudentProfile>
    {
        /// <summary>
        /// Gets the student profile belonging to a user.
        /// </summary>
        /// <param name="UserId">User identifier.</param>
        /// <param name="cancellationToken">Cancels the query.</param>
        /// <returns>The student profile, or null when the user has none.</returns>
        Task<StudentProfile?> GetByUserIdAsync(int UserId, CancellationToken cancellationToken = default);
      

    }
}
