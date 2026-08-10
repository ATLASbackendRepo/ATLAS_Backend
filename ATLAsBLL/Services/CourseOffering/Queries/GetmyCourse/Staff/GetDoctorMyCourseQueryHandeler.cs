using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.CourseOffring;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.DTO.CourseOffring;
using ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Doctor
{
    public class GetStaffMyCourseQueryHandler : IRequestHandler<GetStaffMyCourseQuery, IEnumerable<DoctorCourseOfferingDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStaffMyCourseQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<DoctorCourseOfferingDTO>> Handle(GetStaffMyCourseQuery request, CancellationToken cancellationToken)
        {

            var myCourses = await _unitOfWork.CourseOfferings.GetStaffMyCourseAsync(request.StaffId);

            return myCourses;
        }
    }
}