using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.CourseGrades.Commands.DeleteCourseGrade
{
    public class DeleteCourseGradeCommandHandler : IRequestHandler<DeleteCourseGradeCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCourseGradeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteCourseGradeCommand request, CancellationToken cancellationToken)
        {
            var grade = await _unitOfWork.CourseGrades.GetByIdAsync(request.CG_ID);

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