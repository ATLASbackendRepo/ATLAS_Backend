using ATLAsDCL.UnitOfWork;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.DTO.CourseOffring;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student
{
    public class GetMyCourseQueryHandler : IRequestHandler<GetMyCourseQuery, IEnumerable<CourseOfferingDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMyCourseQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CourseOfferingDTO>> Handle(GetMyCourseQuery request, CancellationToken cancellationToken)
        {
           
            var myCourses = await _unitOfWork.CourseOfferings.GetMyCourseAsync(request.StudentId);

            return myCourses;
        }
    }
}