using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferings.Commands.UpdateCourseOffering
{
    /// <summary>
    /// Update Course Offering Command Handler.
    /// </summary>
    public class UpdateCourseOfferingCommandHandler : IRequestHandler<UpdateCourseOfferingCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCourseOfferingCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
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
