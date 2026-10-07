using ATLAsDCL.UnitOfWork;
using ATLASDomain.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.User.Command.ChangePassword
{
    /// <summary>
    /// Change Passwordcommand Handler.
    /// </summary>
    public class ChangePasswordAdmincommandHandler :IRequestHandler<ChangePasswordAdimincommand  , bool>
    {
        /// <summary>
        /// Minimum length enforced for every new password. Matches the length of
        /// the default password handed to new accounts so the first mandatory
        /// change cannot keep a trivially short secret.
        /// </summary>
        public const int MinimumPasswordLength = 8;

        /// <summary>
        /// unit Of Work.
        /// </summary>
        private readonly IUnitOfWork unitOfWork;
        public ChangePasswordAdmincommandHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(ChangePasswordAdimincommand request, CancellationToken cancellationToken)
        {
            if (request.model.NewPassword != request.model.ConfirmPassword)
                throw new InvalidDataException("New password and confirm password do not match.");
            if (string.IsNullOrWhiteSpace(request.model.NewPassword))
                throw new InvalidDataException("New password is required.");
            if (request.model.NewPassword.Length < MinimumPasswordLength)
                throw new InvalidDataException($"New password must be at least {MinimumPasswordLength} characters long.");
         

            request.user.Password = BCrypt.Net.BCrypt.HashPassword(request.model.NewPassword);
            request.user.MustChangePassword = false;

            // Rotate SecurityStamp so any active JWT issued before the password change is instantly revoked (HIGH-03).
            request.user.SecurityStamp = Guid.NewGuid();

            // Any refresh token issued before the password change is no longer
            // trustworthy (it may predate the mandatory-change flag), so the
            // whole family is revoked and the user signs in again with the new
            // password.
            request.user.RefreshTokenHash = null;
            request.user.PreviousRefreshTokenHash = null;
            request.user.RefreshTokenExpiresAt = null;
            request.user.RefreshTokenRevokedAt = DateTime.UtcNow;

            unitOfWork.Users.Update(request.user);
            var isSuccess = await unitOfWork.SaveChangesAsync(cancellationToken) > 0;

            return isSuccess;

        }
    }
}
