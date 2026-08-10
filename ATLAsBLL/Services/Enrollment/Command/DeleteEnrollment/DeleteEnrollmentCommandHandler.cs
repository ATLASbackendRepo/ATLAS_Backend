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
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(request.EnrollmentID);

            if (enrollment == null)
                throw new System.Collections.Generic.KeyNotFoundException($"Enrollment record with ID {request.EnrollmentID} does not exist.");

            _unitOfWork.Enrollments.Delete(enrollment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}