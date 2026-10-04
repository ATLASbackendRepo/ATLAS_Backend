using ATLAsDCL.UnitOfWork;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.DTO.CourseOffring;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetCourseDetails
{
    /// <summary>
    /// Get Course Details Query Handler.
    /// </summary>
    public class GetCourseDetailsQueryHandler : IRequestHandler<GetCourseDetailsQuery, IEnumerable<CourseofferingDetailsDTO>>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public GetCourseDetailsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<IEnumerable<CourseofferingDetailsDTO>> Handle(GetCourseDetailsQuery request, CancellationToken cancellationToken)
        {
           
               var user = await _unitOfWork.Users.GetByIdAsync(request.userId, cancellationToken);
            if (user == null) throw new Exception();
            IEnumerable<CourseofferingDetailsDTO> courseDetails;

            if (user.Role == ATLASDomain.Entities.enUserRole.Admin || user.Role == ATLASDomain.Entities.enUserRole.SAdmin)
                 courseDetails = await _unitOfWork.CourseOfferings.GetDetailsCourseAsync(request.CourseOfferingId, cancellationToken);
            else
                courseDetails = await _unitOfWork.CourseOfferings.GetMyCourseDetailsAsync(request.CourseOfferingId, request.userId, cancellationToken);

                
                if (courseDetails == null)
                return Enumerable.Empty<CourseofferingDetailsDTO>();
         

            return courseDetails;
        }
    }
}
