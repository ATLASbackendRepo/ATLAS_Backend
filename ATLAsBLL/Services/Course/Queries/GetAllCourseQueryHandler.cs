using ATLAsBLL.Services.Department.Query;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Course;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Course.Queries
{
    /// <summary>
    /// Get All Course Query Handler.
    /// </summary>
    public class GetAllCourseQueryHandler : IRequestHandler<GetAllCourseQuery , List<CourseDTO>>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        public GetAllCourseQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

     /// <summary>
     /// Handle.
     /// </summary>
     public async Task<List<CourseDTO>> Handle(GetAllCourseQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Courses.GetAllCourse(request.courseName, cancellationToken);
        }
    }
}
