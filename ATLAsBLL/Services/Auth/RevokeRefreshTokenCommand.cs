using MediatR;

namespace ATLAsBLL.Services.Auth;

/// <summary>
/// Revoke Refresh Token Command.
/// </summary>
public record RevokeRefreshTokenCommand(int UserId) : IRequest;
