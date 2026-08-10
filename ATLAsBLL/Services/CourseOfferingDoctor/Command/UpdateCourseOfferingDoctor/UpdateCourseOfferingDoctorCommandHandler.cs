using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.UpdateCourseOfferingDoctor
{
    public class UpdateCourseOfferingDoctorCommandHandler : IRequestHandler<UpdateCourseOfferingDoctorCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateCourseOfferingDoctorCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<bool> Handle(UpdateCourseOfferingDoctorCommand request, CancellationToken cancellationToken)
        {
            var record = await _unitOfWork.CourseOfferingDoctors.GetByIdAsync(request.CourseOfferingDoctorId);
            if (record == null) throw new System.Collections.Generic.KeyNotFoundException("Assignment record not found.");

            record.DoctorID = request.NewDoctorId;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}