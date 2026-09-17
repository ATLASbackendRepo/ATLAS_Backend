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
    public class GetAllCourseQueryHandler : IRequestHandler<GetAllCourseQuery , List<CourseDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetAllCourseQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

     public async Task<List<CourseDTO>> Handle(GetAllCourseQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Courses.GetAllCourse(request.courseName);
        }
    }
}
