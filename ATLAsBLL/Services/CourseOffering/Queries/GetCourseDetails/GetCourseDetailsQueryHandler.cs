using ATLAsDCL.UnitOfWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.DTO.CourseOffring;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetCourseDetails
{
    /// <summary>
    /// Get Course Details Query Handler.
    /// </summary>
    public class GetCourseDetailsQueryHandler : IRequestHandler<GetCourseDetailsQuery, IEnumerable<CourseofferingDetailsDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetCourseDetailsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<IEnumerable<CourseofferingDetailsDTO>> Handle(GetCourseDetailsQuery request, CancellationToken cancellationToken)
        {
            var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(request.CourseOfferingId, cancellationToken);
            if (offering == null)
            {
                throw new KeyNotFoundException($"Course offering with ID {request.CourseOfferingId} was not found.");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(request.userId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {request.userId} was not found.");
            }

            if (!_currentUserService.IsSAdmin)
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }

                if (offering.InstitutionID == null || offering.InstitutionID.Value != _currentUserService.InstitutionId.Value)
                {
                    throw new ForbiddenAccessException("User is not authorized to view details for this course offering.");
                }
            }

            IEnumerable<CourseofferingDetailsDTO> courseDetails;

            if (user.Role == ATLASDomain.Entities.enUserRole.Admin || user.Role == ATLASDomain.Entities.enUserRole.SAdmin)
            {
                courseDetails = await _unitOfWork.CourseOfferings.GetDetailsCourseAsync(request.CourseOfferingId, cancellationToken);
            }
            else
            {
                courseDetails = await _unitOfWork.CourseOfferings.GetMyCourseDetailsAsync(request.CourseOfferingId, request.userId, cancellationToken);
            }

            return courseDetails ?? Enumerable.Empty<CourseofferingDetailsDTO>();
        }
    }
}
