using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.DeleteCourseOfferingTA
{
    public class DeleteCourseOfferingTACommandHandler : IRequestHandler<DeleteCourseOfferingTACommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteCourseOfferingTACommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<bool> Handle(DeleteCourseOfferingTACommand request, CancellationToken cancellationToken)
        {
            var record = await _unitOfWork.CourseOfferingTAs.GetByIdAsync(request.CourseOfferingTAId);
            if (record == null) throw new System.Collections.Generic.KeyNotFoundException("TA Assignment record not found.");

            _unitOfWork.CourseOfferingTAs.Delete(record);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}