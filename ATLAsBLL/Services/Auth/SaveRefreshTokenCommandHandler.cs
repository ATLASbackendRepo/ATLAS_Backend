using ATLAsDCL.UnitOfWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Auth
{
    /// <summary>
    /// Save Refresh Token Command Handler.
    /// </summary>
    public class SaveRefreshTokenCommandHandler
    : IRequestHandler<SaveRefreshTokenCommand>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public SaveRefreshTokenCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task Handle(
            SaveRefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users
                .GetByIdAsync(request.UserId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            // Refresh-token rotation with reuse detection:
            // slide the current token into the "previous" slot so a rotated
            // token that is later replayed can be detected and the family revoked.
            user.PreviousRefreshTokenHash = user.RefreshTokenHash;

            user.RefreshTokenHash =
                BCrypt.Net.BCrypt.HashPassword(request.RefreshToken);

            user.RefreshTokenExpiresAt =
                DateTime.UtcNow.AddDays(7);

            user.RefreshTokenRevokedAt = null;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
