using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.DeleteCourseOfferingDoctor
{
    public class DeleteCourseOfferingDoctorCommandHandler : IRequestHandler<DeleteCourseOfferingDoctorCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteCourseOfferingDoctorCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<bool> Handle(DeleteCourseOfferingDoctorCommand request, CancellationToken cancellationToken)
        {
            var record = await _unitOfWork.CourseOfferingDoctors.GetByIdAsync(request.CourseOfferingDoctorId);
            if (record == null) throw new System.Collections.Generic.KeyNotFoundException("Assignment record not found.");

            _unitOfWork.CourseOfferingDoctors.Delete(record);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}