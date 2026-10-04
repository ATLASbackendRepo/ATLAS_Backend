using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.DeleteCourseOfferingTA
{
    /// <summary>
    /// Delete Course Offering TACommand Handler.
    /// </summary>
    public class DeleteCourseOfferingTACommandHandler : IRequestHandler<DeleteCourseOfferingTACommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        public DeleteCourseOfferingTACommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(DeleteCourseOfferingTACommand request, CancellationToken cancellationToken)
        {
            var record = await _unitOfWork.CourseOfferingTAs.GetByIdAsync(request.CourseOfferingTAId, cancellationToken);
            if (record == null) throw new System.Collections.Generic.KeyNotFoundException("TA Assignment record not found.");

            _unitOfWork.CourseOfferingTAs.Delete(record);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
