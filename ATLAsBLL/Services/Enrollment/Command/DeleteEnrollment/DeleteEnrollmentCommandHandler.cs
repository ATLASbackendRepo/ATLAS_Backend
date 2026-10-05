using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Enrollments.Commands.DeleteEnrollment
{
    /// <summary>
    /// Delete Enrollment Command Handler.
    /// </summary>
    public class DeleteEnrollmentCommandHandler : IRequestHandler<DeleteEnrollmentCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public DeleteEnrollmentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(DeleteEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var ISDeleted = await _unitOfWork.Enrollments.DeleteEnrollmentByIdAsync(request.EnrollmentID, cancellationToken);
            return ISDeleted;
        }
    }
}
