using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Semesters.Commands.UpdateSemester
{
    /// <summary>
    /// Update Semester Command Handler.
    /// </summary>
    public class UpdateSemesterCommandHandler : IRequestHandler<UpdateSemesterCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public UpdateSemesterCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(UpdateSemesterCommand request, CancellationToken cancellationToken)
        {
            var semester = await _unitOfWork.Semesters.GetByIdAsync(request.SemesterID);

            if (semester == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Semester with ID {request.SemesterID} was not found.");
            }

            semester.SemesterName = request.SemesterName;
            semester.AcademicYear = request.AcademicYear;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
