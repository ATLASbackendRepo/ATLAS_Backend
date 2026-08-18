using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.UpdateCourseOfferingTA
{
    public class UpdateCourseOfferingTACommandHandler : IRequestHandler<UpdateCourseOfferingTACommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateCourseOfferingTACommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<bool> Handle(UpdateCourseOfferingTACommand request, CancellationToken cancellationToken)
        {
            var record = await _unitOfWork.CourseOfferingTAs.GetByIdAsync(request.CourseOfferingTAId);
            if (record == null) throw new System.Collections.Generic.KeyNotFoundException("TA Assignment record not found.");

            record.TeacherAssistID = request.NewTAID;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}