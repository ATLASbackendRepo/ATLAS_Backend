using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.CourseGrade;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseGrade.Queries
{
    public class GetCourseStudentsQueryHandler : IRequestHandler<GetCourseStudentsQuery, PagedResultDto<StudentCardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCourseStudentsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<StudentCardDto>> Handle(GetCourseStudentsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CourseGrades.GetCourseStudentsPagedAsync(request.QueryModel);
        }
    }
}
