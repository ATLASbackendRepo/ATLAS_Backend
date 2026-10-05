using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Semesters.Commands.DeleteSemester
{
    /// <summary>
    /// Delete Semester Command Handler.
    /// </summary>
    public class DeleteSemesterCommandHandler : IRequestHandler<DeleteSemesterCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public DeleteSemesterCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(DeleteSemesterCommand request, CancellationToken cancellationToken)
        {
            var semester = await _unitOfWork.Semesters.GetByIdAsync(request.SemesterID, cancellationToken);

            if (semester == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Semester with ID {request.SemesterID} does not exist.");
            }

            _unitOfWork.Semesters.Delete(semester);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
