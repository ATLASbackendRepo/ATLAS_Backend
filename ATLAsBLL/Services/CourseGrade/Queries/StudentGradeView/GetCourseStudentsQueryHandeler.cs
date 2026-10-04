using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.CourseGrade;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseGrade.Queries
{
    /// <summary>
    /// Get Course Students Query Handler.
    /// </summary>
    public class GetCourseStudentsQueryHandler : IRequestHandler<GetCourseStudentsQuery, PagedResultDto<StudentCardDto>>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public GetCourseStudentsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<PagedResultDto<StudentCardDto>> Handle(GetCourseStudentsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CourseGrades.GetCourseStudentsPagedAsync(request.QueryModel, cancellationToken);
        }
    }
}
