using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferings.Commands.DeleteCourseOffering
{
    public class DeleteCourseOfferingCommandHandler : IRequestHandler<DeleteCourseOfferingCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCourseOfferingCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteCourseOfferingCommand request, CancellationToken cancellationToken)
        {
            var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(request.CourseOfferingId);

            if (offering == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Course Offering with ID {request.CourseOfferingId} does not exist.");
            }

            _unitOfWork.CourseOfferings.Delete(offering);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}