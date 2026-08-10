using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.CreateCourseOfferingDoctor
{
    public class CreateCourseOfferingDoctorCommandHandler : IRequestHandler<CreateCourseOfferingDoctorCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateCourseOfferingDoctorCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<int> Handle(CreateCourseOfferingDoctorCommand request, CancellationToken cancellationToken)
        {
            var cod = new CourseOfferingDoctor
            {
                CourseOfferingID = request.CourseOfferingId,
                DoctorID = request.DoctorId
            };
            await _unitOfWork.CourseOfferingDoctors.AddAsync(cod);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return cod.CourseOfferingDoctorID;
        }
    }
}