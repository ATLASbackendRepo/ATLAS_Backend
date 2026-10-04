using ATLAsDCL.UnitOfWork;
using MediatR;

namespace ATLAsBLL.Services.Auth;

/// <summary>
/// Save Refresh Token Command Handler.
/// </summary>
/// <remarks>
/// This is the login path. There is no token being rotated away, so the new hash
/// is written unconditionally. Rotation uses <see cref="RotateRefreshTokenCommand"/>
/// instead, which is a compare-and-swap.
/// </remarks>
public sealed class SaveRefreshTokenCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SaveRefreshTokenCommand, bool>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<bool> Handle(
        SaveRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        user.PreviousRefreshTokenHash = user.RefreshTokenHash;
        user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(request.RefreshToken);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        user.RefreshTokenRevokedAt = null;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>
/// Rotate Refresh Token Command Handler (MED-02).
/// </summary>
/// <remarks>
/// The race that used to exist was a read-then-write: the row was read, the
/// presented token was verified with BCrypt, and then the new hash was written
/// with no condition attached. Two requests replaying the same token could both
/// read the same row, both pass the BCrypt check and both write, so two callers
/// each ended up holding a token that the system believed was valid.
/// <para>
/// Here the comparison and the write are one statement, so the database decides
/// which caller wins. The loser sees zero updated rows, reports that the token was
/// reused, and the controller revokes the whole family - which is the correct
/// response to a token that is being replayed.
/// </para>
/// </remarks>
public sealed class RotateRefreshTokenCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RotateRefreshTokenCommand, RefreshRotationResult>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<RefreshRotationResult> Handle(
        RotateRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // An account that has been deactivated or had its family revoked keeps a
        // null current hash, so a stale token cannot be rotated back to life.
        var user = await unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        if (!user.IsActive || user.RefreshTokenRevokedAt is not null)
            return new RefreshRotationResult(false, WasReused: true);

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewRefreshToken);
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var rowsAffected = await unitOfWork.Users.TryRotateRefreshTokenAsync(
            request.UserId,
            request.ExpectedRefreshTokenHash,
            newHash,
            expiresAt,
            cancellationToken);

        // Zero rows means the token was already rotated away between the read and
        // the write. Somebody is replaying a spent token, so the caller must not
        // be handed a new one.
        return rowsAffected == 1
            ? new RefreshRotationResult(Succeeded: true, WasReused: false)
            : new RefreshRotationResult(Succeeded: false, WasReused: true);
    }
}