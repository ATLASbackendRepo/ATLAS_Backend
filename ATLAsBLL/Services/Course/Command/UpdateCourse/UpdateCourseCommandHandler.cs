using MediatR;
using ATLAsDCL.UnitOfWork;
using System.IO; 
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Features.Courses.Commands.UpdateCourse
{
    /// <summary>
    /// Update Course Command Handler.
    /// </summary>
    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCourseCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(request.UpdateCourseDTO.Id, cancellationToken);

            if (course == null)
            {
               
                throw new System.Collections.Generic.KeyNotFoundException($"Course with ID {request.UpdateCourseDTO.Id} was not found.");
            }
            if (string.IsNullOrEmpty(request.UpdateCourseDTO.CourseName))
            {
                throw new ArgumentNullException();
            }

            course.C_Name = request.UpdateCourseDTO.CourseName;
            course.Credit_Hours = request.UpdateCourseDTO.CreditHours;
            course.IsPractical = request.UpdateCourseDTO.IsPractical;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
