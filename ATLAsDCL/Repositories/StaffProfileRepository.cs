using ATLAS.Domain.Entities;
using ATLAsDCL.Data;
using ATLASDomain.InterFaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ATLAS.Persistence.Repositories
{
    /// <summary>
    /// Staff Profile Repository.
    /// </summary>
    /// <remarks>
    /// LOW-08: derives from <see cref="GenericRepository{TEntity}"/> like the other
    /// repositories instead of re-implementing the generic CRUD surface, so the
    /// CancellationToken support added there is inherited rather than duplicated.
    /// </remarks>
    public class StaffProfileRepository
        : GenericRepository<StaffProfile>, IStaffProfileRepository
    {
        public StaffProfileRepository(AtLASDbContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Get By User Id Async.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="cancellationToken">Cancels the query.</param>
        /// <remarks>
        /// The profile and its user are returned tracked so a caller that edits
        /// them only writes the columns it actually changed. The previous detached
        /// projection forced the caller to call Update, which rewrote the whole
        /// graph: StaffProfiles.UserID was written as 0 and the user's password and
        /// refresh-token hash were overwritten with values read earlier.
        /// </remarks>
        public async Task<StaffProfile?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.StaffProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserID == userId, cancellationToken);
        }
    }
}
