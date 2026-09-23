using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Courses.Commands.CreateCourse
{
    /// <summary>
    /// Create Course Command Handler.
    /// </summary>
    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, int>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public CreateCourseCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<int> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(request.CreateCourseDTO.CourseName))
            {
                throw new ArgumentNullException();
            }


            var course = new Course
            {
                C_Name = request.CreateCourseDTO.CourseName,
                Credit_Hours = request.CreateCourseDTO.CreditHours,
                IsPractical = request.CreateCourseDTO.IsPractical
            };

            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return course.CourseID;
        }
    }
}
