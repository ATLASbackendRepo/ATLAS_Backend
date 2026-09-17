using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Enrollments.Commands.DeleteEnrollment
{
    public class DeleteEnrollmentCommandHandler : IRequestHandler<DeleteEnrollmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteEnrollmentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var ISDeleted = await _unitOfWork.Enrollments.DeleteEnrollmentByIdAsync(request.EnrollmentID);
            return ISDeleted;
        }
    }
}