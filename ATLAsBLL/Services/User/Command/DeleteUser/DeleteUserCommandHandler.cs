using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Users.Commands.DeleteUser
{
    /// <summary>
    /// Delete User Command Handler.
    /// </summary>
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetUserByAcademicIDAsync(request.AcademicId);

            if (user == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"User with ID {request.AcademicId} does not exist.");
            }

            user.IsActive = false;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
