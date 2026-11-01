using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Course;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Services.Course.Queries
{
    /// <summary>
    /// Get All Course Query Handler.
    /// </summary>
    public class GetAllCourseQueryHandler : IRequestHandler<GetAllCourseQuery, List<CourseDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetAllCourseQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<List<CourseDTO>> Handle(GetAllCourseQuery request, CancellationToken cancellationToken)
        {
            int? institutionFilter = null;
            if (!_currentUserService.IsSAdmin)
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }
                institutionFilter = _currentUserService.InstitutionId.Value;
            }

            return await _unitOfWork.Courses.GetAllCourse(request.courseName, institutionFilter, cancellationToken);
        }
    }
}
