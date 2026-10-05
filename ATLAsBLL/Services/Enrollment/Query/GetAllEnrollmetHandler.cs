using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.DTO.Enrollment;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Enrollment.Query
{
    /// <summary>
    /// Get All Enrollmet Handler.
    /// </summary>
    public  class GetAllEnrollmetHandler : IRequestHandler<GetEnrollment , PagedResultDto<CourseEnrollmentSummaryDto>>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public GetAllEnrollmetHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<PagedResultDto<CourseEnrollmentSummaryDto>> Handle(GetEnrollment request, CancellationToken cancellationToken)
        {
            var EnrollmentOverview = await _unitOfWork.Enrollments.GetEnrollmentsOverviewAsync(request.searchKeyword, request.doctorId
                                                                                                , request.pageIndex, cancellationToken: cancellationToken);
            if (EnrollmentOverview == null)
                throw new Exception();

            return EnrollmentOverview;
        }
    }
}
