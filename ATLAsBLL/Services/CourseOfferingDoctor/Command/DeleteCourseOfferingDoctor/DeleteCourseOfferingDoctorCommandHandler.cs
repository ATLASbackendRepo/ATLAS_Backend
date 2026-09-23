using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.DeleteCourseOfferingDoctor
{
    /// <summary>
    /// Delete Course Offering Doctor Command Handler.
    /// </summary>
    public class DeleteCourseOfferingDoctorCommandHandler : IRequestHandler<DeleteCourseOfferingDoctorCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        public DeleteCourseOfferingDoctorCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        /// <summary>
        /// Handle.
        /// </summary>
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
