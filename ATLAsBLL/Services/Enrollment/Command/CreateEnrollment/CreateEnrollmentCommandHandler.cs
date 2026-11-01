using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment
{
    /// <summary>
    /// Create Enrollment Command Handler.
    /// </summary>
    public class CreateEnrollmentCommandHandler : IRequestHandler<CreateEnrollmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateEnrollmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// Handles student enrollment with institution scoping.
        /// </summary>
        public async Task<bool> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            if (request.BulkCreateEnrollmentDTO1 == null)
            {
                throw new ArgumentException("Enrollment details are required.");
            }

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

            var isInserted = await _unitOfWork.Enrollments.AddSingleStudentToEnrollmentAsync(
                request.AcdimicID,
                request.BulkCreateEnrollmentDTO1,
                request.CreatedByUserId,
                institutionId,
                cancellationToken);

            return isInserted;
        }
    }
}
