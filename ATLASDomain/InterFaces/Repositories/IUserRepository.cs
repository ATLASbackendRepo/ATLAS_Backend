using ATLASDomain.DTO.Staff;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// IUser Repository.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// Gets a user by username.
    /// </summary>
    /// <param name="userName">Username to match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user, or null when no account has that username.</returns>
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets a user by identifier.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user, or null when it does not exist.</returns>
    Task<User?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets a filtered, paged list of the staff accounts holding a given role.
    /// </summary>
    /// <param name="role">Role to filter on.</param>
    /// <param name="filter">Paging and filtering criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching staff accounts.</returns>
     Task<IEnumerable<StaffDto>> GetAllStaffAsync(enUserRole role, StaffFilterDTO filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user account behind an academic number.
    /// </summary>
    /// <param name="academicID">Academic number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user, or null when no account has that number.</returns>
    Task<User?> GetUserByAcademicIDAsync(string academicID, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks whether a username is already taken.
    /// </summary>
    /// <param name="userName">Username to test.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when an account already uses that username.</returns>
    Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user id behind an academic number.
    /// </summary>
    /// <param name="AcdimicID">Academic number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user id, or null when no account has that number.</returns>
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
