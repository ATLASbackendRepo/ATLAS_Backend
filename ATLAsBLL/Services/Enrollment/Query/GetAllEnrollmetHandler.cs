using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.DTO.Enrollment;
using ATLAsBLL.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Enrollment.Query
{
    /// <summary>
    /// Get All Enrollment Handler.
    /// Scopes overview queries to the caller's institution for non-SAdmin users.
    /// </summary>
    public class GetAllEnrollmetHandler : IRequestHandler<GetEnrollment, PagedResultDto<CourseEnrollmentSummaryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetAllEnrollmetHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// Handles retrieving enrollment summaries scoped by institution.
        /// </summary>
        public async Task<PagedResultDto<CourseEnrollmentSummaryDto>> Handle(GetEnrollment request, CancellationToken cancellationToken)
        {
            int? institutionId;
            if (_currentUserService.IsSAdmin)
            {
                institutionId = null;
            }
            else
            {
                var userInstId = _currentUserService.InstitutionId;
                if (!userInstId.HasValue || userInstId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User is not associated with any institution.");
                }
                institutionId = userInstId.Value;
            }

            var enrollmentOverview = await _unitOfWork.Enrollments.GetEnrollmentsOverviewAsync(
                request.searchKeyword,
                request.doctorId,
                request.pageIndex,
                institutionId: institutionId,
                cancellationToken: cancellationToken);

            return enrollmentOverview ?? new PagedResultDto<CourseEnrollmentSummaryDto>
            {
                Items = new List<CourseEnrollmentSummaryDto>(),
                TotalCount = 0,
                PageNumber = request.pageIndex,
                PageSize = 10
            };
        }
    }
}
