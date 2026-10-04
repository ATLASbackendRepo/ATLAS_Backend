using ATLAS.Domain.Entities;
using ATLAsDCL.Data;
using ATLASDomain.InterFaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ATLAS.Persistence.Repositories
{
    /// <summary>
    /// Student Profile Repository.
    /// </summary>
    /// <remarks>
    /// LOW-08: derives from <see cref="GenericRepository{TEntity}"/> like the other
    /// repositories instead of re-implementing the generic CRUD surface, so the
    /// CancellationToken support added there is inherited rather than duplicated.
    /// </remarks>
    public class StudentProfileRepository
        : GenericRepository<StudentProfile>, IStudentProfileRepository
    {
        public StudentProfileRepository(AtLASDbContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Get By User Id Async.
        /// </summary>
        /// <param name="studentId">Student identifier.</param>
        /// <param name="cancellationToken">Cancels the query.</param>
        /// <remarks>
        /// The profile, its student and that student's user are returned tracked so
        /// a caller that edits them only writes the columns it actually changed. A
        /// detached graph forces the caller to call Update, which rewrites every
        /// property, including the user's password and refresh-token hash.
        /// </remarks>
        public async Task<StudentProfile?> GetByUserIdAsync(int studentId, CancellationToken cancellationToken = default)
        {
            return await _context.StudentProfiles
                .Include(p => p.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.StudentID == studentId, cancellationToken);
        }
    }
}
