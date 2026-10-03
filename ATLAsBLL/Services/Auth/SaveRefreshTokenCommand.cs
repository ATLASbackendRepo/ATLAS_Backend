using MediatR;

namespace ATLAsBLL.Services.Auth;

/// <summary>
/// Save Refresh Token Command.
/// </summary>
/// <param name="UserId">Account the token is issued for.</param>
/// <param name="RefreshToken">The new refresh token, in plaintext.</param>
/// <remarks>
/// MED-02: this is the login path, where there is no current token to rotate
/// from. The handler writes the new hash unconditionally and stamps a fresh
/// security value. Rotation uses
/// <see cref="RotateRefreshTokenCommand"/> instead, which is a compare-and-swap.
/// </remarks>
public record SaveRefreshTokenCommand(
 int UserId,
 string RefreshToken
) : ICommand<bool>, IWriteCommand;

/// <summary>
/// Rotate Refresh Token Command (MED-02).
/// </summary>
/// <param name="UserId">Account the token belongs to.</param>
/// <param name="ExpectedRefreshTokenHash">
/// Hash of the token the caller presented, captured when the row was read. The
/// update is conditional on this value still being the current one, so exactly
/// one of two concurrent refreshes can succeed.
/// </param>
/// <param name="NewRefreshToken">The replacement token, in plaintext.</param>
/// <remarks>
/// The previous implementation read the row, verified the token with BCrypt and
/// then wrote the new hash with no condition, so two requests replaying the same
/// token both succeeded and the second silently clobbered the first. Making the
/// write conditional turns that into a single winner.
/// </remarks>
public record RotateRefreshTokenCommand(
 int UserId,
 string ExpectedRefreshTokenHash,
 string NewRefreshToken
) : ICommand<RefreshRotationResult>, IWriteCommand;

/// <summary>
/// Outcome of a refresh-token rotation (MED-02).
/// </summary>
/// <param name="Succeeded">
/// True when this caller won the race and its new token is the live one.
/// </param>
/// <param name="WasReused">
/// True when the token had already been rotated away, which means somebody is
/// replaying a spent token and the whole family must be revoked.
/// </param>
public record RefreshRotationResult(bool Succeeded, bool WasReused);