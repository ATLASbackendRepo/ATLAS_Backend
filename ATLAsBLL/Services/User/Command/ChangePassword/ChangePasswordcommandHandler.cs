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
    internal class ChangePasswordcommandHandler :IRequestHandler<ChangePasswordcommand ,bool>
    {
        private readonly IUnitOfWork unitOfWork;
        public ChangePasswordcommandHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ChangePasswordcommand request, CancellationToken cancellationToken)
        {
            if (request.model.NewPassword != request.model.ConfirmPassword)
                throw new InvalidDataException("New password and confirm password do not match.");
            if (!BCrypt.Net.BCrypt.Verify(request.model.OldPassword, request.user.Password))
                throw new UnauthorizedAccessException("The current password is incorrect.");

            request.user.Password = BCrypt.Net.BCrypt.HashPassword(request.model.NewPassword);
            request.user.MustChangePassword = false;

            
            unitOfWork.Users.Update(request.user);
            var isSuccess = await unitOfWork.SaveChangesAsync(cancellationToken) > 0;

            return isSuccess;

        }
    }
}
