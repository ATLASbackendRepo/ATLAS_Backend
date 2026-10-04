using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseGrades.Commands.DeleteCourseGrade
{
    /// <summary>
    /// Delete Course Grade Command Handler.
    /// </summary>
    public class DeleteCourseGradeCommandHandler : IRequestHandler<DeleteCourseGradeCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCourseGradeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(DeleteCourseGradeCommand request, CancellationToken cancellationToken)
        {
            var grade = await _unitOfWork.CourseGrades.GetByIdAsync(request.CG_ID, cancellationToken);

            if (grade == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Course Grade record with ID {request.CG_ID} does not exist.");
            }

            _unitOfWork.CourseGrades.Delete(grade);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
