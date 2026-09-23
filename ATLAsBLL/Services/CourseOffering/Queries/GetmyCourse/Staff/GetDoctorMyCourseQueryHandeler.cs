using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.CourseOffring;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Doctor
{
    /// <summary>
    /// Get Staff My Course Query Handler.
    /// </summary>
    public class GetStaffMyCourseQueryHandler : IRequestHandler<GetStaffMyCourseQuery, IEnumerable<DoctorCourseOfferingDTO>>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public GetStaffMyCourseQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<IEnumerable<DoctorCourseOfferingDTO>> Handle(GetStaffMyCourseQuery request, CancellationToken cancellationToken)
        {

            var myCourses = await _unitOfWork.CourseOfferings.GetStaffMyCourseAsync(request.StaffId);

            return myCourses;
        }
    }
}
