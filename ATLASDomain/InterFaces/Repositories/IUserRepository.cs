using ATLASDomain.DTO.Staff;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// IUser Repository.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByUserIdAsync(int userId);
     Task<IEnumerable<StaffDto>> GetAllStaffAsync(enUserRole role, StaffFilterDTO filter);

    Task<User?> GetUserByAcademicIDAsync(string academicID);
    Task<bool> UserNameExistsAsync(string userName);

    Task<int?> GetUserIdByAcdimicId(string AcdimicID, CancellationToken cancellationToken);

    /// <summary>
    /// Rotates the refresh token with an atomic compare-and-swap (MED-02).
    /// </summary>
    /// <param name="userId">Account the token belongs to.</param>
    /// <param name="expectedHash">
    /// Hash that the row's current refresh token is expected to hold. The
    /// update only applies while that is still true.
    /// </param>
    /// <param name="newHash">Hash of the replacement token.</param>
    /// <param name="expiresAt">Expiry of the replacement token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The number of rows updated: 1 when this caller won the rotation, 0 when
    /// the token had already been rotated away by somebody else.
    /// </returns>
    /// <remarks>
    /// Doing the comparison inside the UPDATE is what closes the race. A
    /// read-then-write lets two requests carrying the same token both observe
    /// the old hash and both succeed, issuing two live tokens from one.
    /// </remarks>
    Task<int> TryRotateRefreshTokenAsync(
        int userId,
        string expectedHash,
        string newHash,
        DateTime expiresAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates the security stamp of an account, invalidating every access token
    /// already issued for it (MED-01).
    /// </summary>
    /// <param name="userId">Account to invalidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of rows updated.</returns>
    Task<int> BumpSecurityStampAsync(
        int userId,
        CancellationToken cancellationToken = default);

}
