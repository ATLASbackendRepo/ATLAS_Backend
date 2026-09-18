using ATLAsDCL.UnitOfWork;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.DTO.CourseOffring;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetCourseDetails
{
    public class GetCourseDetailsQueryHandler : IRequestHandler<GetCourseDetailsQuery, IEnumerable<CourseofferingDetailsDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCourseDetailsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CourseofferingDetailsDTO>> Handle(GetCourseDetailsQuery request, CancellationToken cancellationToken)
        {
           
               var user = await _unitOfWork.Users.GetByIdAsync(request.userId);
            if (user == null) throw new Exception();
            IEnumerable<CourseofferingDetailsDTO> courseDetails;

            if (user.Role == ATLASDomain.Entities.enUserRole.Admin || user.Role == ATLASDomain.Entities.enUserRole.SAdmin)
                 courseDetails = await _unitOfWork.CourseOfferings.GetDetailsCourseAsync(request.CourseOfferingId);
            else
                courseDetails = await _unitOfWork.CourseOfferings.GetMyCourseDetailsAsync(request.CourseOfferingId, request.userId);

                
                if (courseDetails == null)
                return Enumerable.Empty<CourseofferingDetailsDTO>();
         

            return courseDetails;
        }
    }
}
