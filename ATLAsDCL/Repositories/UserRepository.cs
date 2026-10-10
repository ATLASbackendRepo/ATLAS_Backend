using ATLAsDCL.Data;
using ATLASDomain.DTO.Staff;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// User Repository.
/// </summary>
public class UserRepository
    : GenericRepository<User>, IUserRepository
{
    public UserRepository(AtLASDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Get By User Name Async.
    /// </summary>
    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.FullName == userName, cancellationToken);
    }
    /// <summary>
    /// Get By User Id Async.
    /// </summary>
    public async Task<User?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
    }


    /// <summary>
    /// Get User By Academic IDAsync.
    /// </summary>
    public async Task<User?> GetUserByAcademicIDAsync(string academicID, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u=> u.AcademicId  == academicID, cancellationToken);
    }


    /// <summary>
    /// User Name Exists Async.
    /// </summary>
    public async Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.FullName == userName, cancellationToken);
    }

    /// <summary>
    /// Get All Staff Async.
    /// </summary>
    public async Task<IEnumerable<StaffDto>> GetAllStaffAsync(enUserRole role, StaffFilterDTO filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .AsNoTracking()
            .AsQueryable();

        if(!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(u => u.FullName.Contains(filter.Search) || (u.AcademicId != null && u.AcademicId.Contains(filter.Search)));
        }
        if (filter.DepartmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == filter.DepartmentId.Value);
        }
        if (filter.Status.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.Status.Value);
        }
        if (filter.InstitutionId.HasValue)
        {
            query = query.Where(u => u.InstitutionID == filter.InstitutionId.Value);
        }

        // MED-12: the staff list is bounded for the same reason as the student list.
        // Without this the query returned every account in the system, including
        // the academic id of each one, to a single caller.
        var (pageNumber, pageSize) = filter.Normalize();

        var staffList = query
            .Where(u => u.Role == role)
            .Select(u => new StaffDto
            {
                FullName = u.FullName,
                AcadimicId = u.AcademicId,

                NumberOfCourses = role == enUserRole.Doctor
                    ? _context.CourseOfferingDoctors
                        .Count(c => c.DoctorID == u.UserId)

                    : role == enUserRole.TeachingAssistant
                        ? _context.CourseOfferingTAs
                            .Count(c => c.TeacherAssistID == u.UserId)

                        : 0,

                departmentname = u.Department != null ? u.Department.D_Name : string.Empty,
                IsActive = u.IsActive
            })
            // A deterministic sort keeps paging stable across requests.
            .OrderBy(u => u.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        return await staffList.ToListAsync(cancellationToken);
    }
  

    /// <summary>
    /// Get User Id By Acdimic Id.
    /// </summary>
    public async Task<int?> GetUserIdByAcdimicId(string AcdimicID, CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.AcademicId == AcdimicID)
            .Select(u => (int?)u.UserId)
            .FirstOrDefaultAsync(cancellationToken);
     
    }

    /// <summary>
    /// Rotate Refresh Token Async (MED-02).
    /// </summary>
    /// <param name="userId">Account the token belongs to.</param>
    /// <param name="expectedHash">Hash the row is expected to hold right now.</param>
    /// <param name="newHash">Hash of the replacement token.</param>
    /// <param name="expiresAt">Expiry of the replacement token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>1 when this caller won the rotation, 0 when somebody else did.</returns>
    /// <remarks>
    /// The comparison lives inside the UPDATE statement rather than in C#. A
    /// read-then-write lets two requests replaying the same token both observe
    /// the old hash and both proceed, which issues two live tokens from one and
    /// lets the second silently clobber the first. Doing the compare-and-swap in
    /// a single statement means the database arbitrates: only one row matches the
    /// expected hash, so only one caller can see a row count of 1.
    /// <para>
    /// The spent hash is kept in <c>PreviousRefreshTokenHash</c> so a later
    /// replay can still be recognised as reuse rather than as a bad password.
    /// </para>
    /// </remarks>
    public async Task<int> TryRotateRefreshTokenAsync(
        int userId,
        string expectedHash,
        string newHash,
        DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        // ExecuteUpdate issues a single UPDATE ... WHERE, so the read of the
        // current hash and the write of the new one cannot be interleaved by a
        // competing refresh.
        return await _context.Users
            .Where(u => u.UserId == userId && u.RefreshTokenHash == expectedHash)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(u => u.PreviousRefreshTokenHash, u => u.RefreshTokenHash)
                    .SetProperty(u => u.RefreshTokenHash, newHash)
                    .SetProperty(u => u.RefreshTokenExpiresAt, expiresAt)
                    .SetProperty(u => u.RefreshTokenRevokedAt, (DateTime?)null),
                cancellationToken);
    }

    /// <summary>
    /// Bump Security Stamp Async (MED-01).
    /// </summary>
    /// <param name="userId">Account to invalidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>1 when the account existed and its stamp was replaced.</returns>
    /// <remarks>
    /// A new <see cref="Guid"/> is generated by SQL Server, so two concurrent
    /// bumps cannot collide and no two accounts can ever share a stamp. Every
    /// access token that was issued before the bump carries the old value and is
    /// rejected by the token-validation event on the very next request, which is
    /// what makes logout, deactivation and a role change take effect
    /// immediately rather than when the token happens to expire.
    /// </remarks>
    public async Task<int> BumpSecurityStampAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            if (user is null) return 0;
            user.SecurityStamp = Guid.NewGuid();
            return await _context.SaveChangesAsync(cancellationToken);
        }

        return await _context.Users
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(
                // The new value is generated per row inside SQL, so two concurrent
                // bumps cannot collide and each account gets its own value.
                setters => setters.SetProperty(u => u.SecurityStamp, u => Guid.NewGuid()),
                cancellationToken);
    }
}
