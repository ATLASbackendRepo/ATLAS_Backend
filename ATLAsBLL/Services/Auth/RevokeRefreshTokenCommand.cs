namespace ATLAsBLL.Services.Auth;

/// <summary>
/// Revoke Refresh Token Command.
/// </summary>
/// <param name="UserId">Account whose refresh-token family is revoked.</param>
/// <remarks>
/// MED-01: revoking is not enough on its own. The handler also rotates the
/// security stamp, which invalidates every access token the account already
/// holds, so a logout takes effect on the next request instead of when the
/// access token happens to expire.
/// </remarks>
public record RevokeRefreshTokenCommand(int UserId) : ICommand<bool>, IWriteCommand;