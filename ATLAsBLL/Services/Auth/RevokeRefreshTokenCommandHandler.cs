using ATLAsDCL.UnitOfWork;
using MediatR;

namespace ATLAsBLL.Services.Auth;

public sealed class RevokeRefreshTokenCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RevokeRefreshTokenCommand>
{
    public async Task Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("User not found.");
        user.RefreshTokenRevokedAt = DateTime.UtcNow;
        user.RefreshTokenHash = null;
        user.PreviousRefreshTokenHash = null;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
