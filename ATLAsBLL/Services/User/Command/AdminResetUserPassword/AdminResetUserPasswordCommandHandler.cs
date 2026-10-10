using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.User.Command.AdminResetUserPassword
{
    /// <summary>
    /// Admin Reset User Password Command Handler.
    /// Safely handles target password resets by Admins and SuperAdmins.
    /// </summary>
    public class AdminResetUserPasswordCommandHandler : IRequestHandler<AdminResetUserPasswordCommand, bool>
    {
        /// <summary>
        /// Minimum length enforced for new passwords.
        /// </summary>
        public const int MinimumPasswordLength = 8;

        private readonly IUnitOfWork _unitOfWork;

        public AdminResetUserPasswordCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(AdminResetUserPasswordCommand request, CancellationToken cancellationToken)
        {
            if (request.NewPassword != request.ConfirmPassword)
                throw new InvalidDataException("New password and confirm password do not match.");

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new InvalidDataException("New password is required.");

            if (request.NewPassword.Length < MinimumPasswordLength)
                throw new InvalidDataException($"New password must be at least {MinimumPasswordLength} characters long.");

            var targetUser = await _unitOfWork.Users.GetUserByAcademicIDAsync(request.TargetAcademicId, cancellationToken);
            if (targetUser is null)
                throw new KeyNotFoundException($"User with academic ID '{request.TargetAcademicId}' was not found.");

            if (targetUser.UserId == request.PerformingUserId)
                throw new InvalidOperationException("Use the self-service password change endpoint to update your own password.");

            if (request.PerformingUserRole == enUserRole.Admin)
            {
                if (targetUser.InstitutionID != request.PerformingUserInstitutionId)
                    throw new UnauthorizedAccessException("Administrators cannot reset passwords for users in another institution.");

                if (targetUser.Role == enUserRole.Admin || targetUser.Role == enUserRole.SAdmin)
                    throw new UnauthorizedAccessException("Administrators cannot reset passwords of other Admin or SuperAdmin accounts.");
            }

            targetUser.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            targetUser.MustChangePassword = true;
            targetUser.SecurityStamp = Guid.NewGuid();
            targetUser.RefreshTokenHash = null;
            targetUser.PreviousRefreshTokenHash = null;
            targetUser.RefreshTokenExpiresAt = null;
            targetUser.RefreshTokenRevokedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(targetUser);
            var saved = await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
            if (saved)
            {
                await _unitOfWork.Users.BumpSecurityStampAsync(targetUser.UserId, cancellationToken);
            }
            return saved;
        }
    }
}
