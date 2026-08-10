using ATLAsDCL.UnitOfWork;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Features.Courses.Commands.DeleteCourse
{
    public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCourseCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseID);

            if (course == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Course with ID {request.CourseID} does not exist.");
            }

            _unitOfWork.Courses.Delete(course);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}