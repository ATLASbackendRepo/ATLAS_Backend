using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest
{
    /// <summary>
    /// Update Grade Appeal Request Command Handler.
    /// </summary>
    public class UpdateGradeAppealRequestCommandHandler
        : IRequestHandler<UpdateGradeAppealRequestCommand, bool>
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateGradeAppealRequestCommandHandler(
            IUnitOfWork unitofwork,
            ICurrentUserService currentUserService)
        {
            _unitofwork = unitofwork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(
            UpdateGradeAppealRequestCommand command,
            CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsSAdmin)
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }

                if (_currentUserService.UserId.HasValue && command.DoctorUserId != _currentUserService.UserId.Value)
                {
                    throw new ForbiddenAccessException("Cannot update appeal on behalf of another user.");
                }
            }

            return await _unitofwork.GradeAppealRequests.UpdateAsync(
                command.RequestId,
                command.DoctorUserId,
                command.Request,
                _currentUserService.InstitutionId,
                _currentUserService.IsSAdmin,
                cancellationToken);
        }
    }
}
