using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferings.Commands.UpdateCourseOffering
{
    public class UpdateCourseOfferingCommandHandler : IRequestHandler<UpdateCourseOfferingCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCourseOfferingCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateCourseOfferingCommand request, CancellationToken cancellationToken)
        {
            var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(request.CourseOfferingId);

            if (offering == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Course Offering with ID {request.CourseOfferingId} was not found.");
            }

            offering.CourseID = request.CourseId;
            offering.SemesterID = request.SemesterId;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}