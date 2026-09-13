using MediatR;

namespace ATLAsBLL.Services.Auth;

public record RevokeRefreshTokenCommand(int UserId) : IRequest;
