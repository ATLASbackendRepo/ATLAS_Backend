using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Features.Enrollments.Commands.DeleteEnrollment
{
    /// <summary>
    /// Delete Enrollment Command Handler.
    /// </summary>
    public class DeleteEnrollmentCommandHandler : IRequestHandler<DeleteEnrollmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteEnrollmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// Handles enrollment deletion with institution scoping.
        /// </summary>
        public async Task<bool> Handle(DeleteEnrollmentCommand request, CancellationToken cancellationToken)
        {
            int? institutionId;
            if (_currentUserService.IsSAdmin)
            {
                institutionId = null;
            }
            else
            {
                var userInstId = _currentUserService.InstitutionId;
                if (!userInstId.HasValue || userInstId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User is not associated with any institution.");
                }
                institutionId = userInstId.Value;
            }

            var isDeleted = await _unitOfWork.Enrollments.DeleteEnrollmentByIdAsync(
                request.EnrollmentID,
                institutionId,
                cancellationToken);

            return isDeleted;
        }
    }
}
