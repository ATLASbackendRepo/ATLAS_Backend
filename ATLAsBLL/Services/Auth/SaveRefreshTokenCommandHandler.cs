using ATLAsDCL.UnitOfWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Auth
{
    public class SaveRefreshTokenCommandHandler
    : IRequestHandler<SaveRefreshTokenCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SaveRefreshTokenCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(
            SaveRefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users
                .GetByIdAsync(request.UserId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.RefreshTokenHash =
                BCrypt.Net.BCrypt.HashPassword(request.RefreshToken);

            user.RefreshTokenExpiresAt =
                DateTime.UtcNow.AddDays(7);

            user.RefreshTokenRevokedAt = null;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
