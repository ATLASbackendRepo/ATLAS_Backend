using ATLAsDCL.UnitOfWork;
using MediatR;

namespace ATLAsBLL.Services.Auth;

/// <summary>
/// Revoke Refresh Token Command Handler.
/// </summary>
/// <remarks>
/// MED-01: revoking the refresh token on its own is not enough. The access token
/// already in the caller's browser stays cryptographically valid until it
/// expires, so a sign-out (or a deactivation, or a role change that goes through
/// this path) would leave up to a full expiry window of live access. Rotating
/// the security stamp closes that window: the stamp travels inside the access
/// token, so every token minted before the revoke stops validating immediately.
/// </remarks>
public sealed class RevokeRefreshTokenCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RevokeRefreshTokenCommand, bool>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<bool> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("User not found.");

        user.RefreshTokenRevokedAt = DateTime.UtcNow;
        user.RefreshTokenHash = null;
        user.PreviousRefreshTokenHash = null;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate the access tokens too. This is a separate statement so the
        // caller's own tracked entity does not have to be reloaded and so it
        // also works when the caller holds no tracked user row.
        var bumped = await unitOfWork.Users.BumpSecurityStampAsync(
            request.UserId,
            cancellationToken);

        if (bumped == 0)
            throw new KeyNotFoundException("User not found.");

        return true;
    }
}