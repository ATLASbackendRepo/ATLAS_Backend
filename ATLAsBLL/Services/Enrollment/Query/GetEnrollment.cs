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
    /// Get Enrollment.
    /// </summary>
    public record GetEnrollment(string? searchKeyword = null,
      int? doctorId = null,
      int pageIndex = 1) : IRequest<PagedResultDto<CourseEnrollmentSummaryDto>>;
   
}
