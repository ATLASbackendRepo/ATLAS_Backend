using ATLAsDCL.UnitOfWork;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.DTO.CourseOffring;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student
{
    /// <summary>
    /// Get My Course Query Handler.
    /// </summary>
    public class GetMyCourseQueryHandler : IRequestHandler<GetMyCourseQuery, IEnumerable<CourseOfferingDTO>>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public GetMyCourseQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<IEnumerable<CourseOfferingDTO>> Handle(GetMyCourseQuery request, CancellationToken cancellationToken)
        {
           
            var myCourses = await _unitOfWork.CourseOfferings.GetMyCourseAsync(request.UserId, cancellationToken);

            return myCourses;
        }
    }
}
