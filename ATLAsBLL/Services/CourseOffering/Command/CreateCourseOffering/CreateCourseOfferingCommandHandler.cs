using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferings.Commands.CreateCourseOffering
{
    public class CreateCourseOfferingCommandHandler : IRequestHandler<CreateCourseOfferingCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateCourseOfferingCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateCourseOfferingCommand request, CancellationToken cancellationToken)
        {
            // ممكن تشيك الأول لو المادة دي اتفتحت أصلاً في السمستر ده قبل كده عشان تمنع التكرار (قدام في الـ Business Rules)

            var offering = new CourseOffering
            {
                CourseID = request.CourseId,
                SemesterID = request.SemesterId
            };

            await _unitOfWork.CourseOfferings.AddAsync(offering);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return offering.CourseOfferingID;
        }
    }
}