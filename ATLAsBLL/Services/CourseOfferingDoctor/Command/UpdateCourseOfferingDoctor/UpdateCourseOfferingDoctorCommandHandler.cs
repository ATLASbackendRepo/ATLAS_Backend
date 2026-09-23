using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.UpdateCourseOfferingDoctor
{
    /// <summary>
    /// Update Course Offering Doctor Command Handler.
    /// </summary>
    public class UpdateCourseOfferingDoctorCommandHandler : IRequestHandler<UpdateCourseOfferingDoctorCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        public UpdateCourseOfferingDoctorCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        /// <summary>
        /// Handle.
        /// </summary>
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
