using MediatR;
using ATLAsDCL.UnitOfWork;
using System.IO; 
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Features.Courses.Commands.UpdateCourse
{
    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCourseCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseID);

            if (course == null)
            {
               
                throw new System.Collections.Generic.KeyNotFoundException($"Course with ID {request.CourseID} was not found.");
            }

            course.C_Name = request.CourseName;
            course.Credit_Hours = request.CreditHours;
            course.IsPractical = request.IsPractical;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}