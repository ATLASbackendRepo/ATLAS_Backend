using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.CreateCourseOfferingTA
{
    /// <summary>
    /// Create Course Offering TACommand Handler.
    /// </summary>
    public class CreateCourseOfferingTACommandHandler : IRequestHandler<CreateCourseOfferingTACommand, int>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        public CreateCourseOfferingTACommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<int> Handle(CreateCourseOfferingTACommand request, CancellationToken cancellationToken)
        {
            var cot = new CourseOfferingTA
            {
                CourseOfferingID = request.CourseOfferingId,
                TeacherAssistID = request.TAID
            };
            await _unitOfWork.CourseOfferingTAs.AddAsync(cot);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return cot.CourseOfferingTAID;
        }
    }
}
