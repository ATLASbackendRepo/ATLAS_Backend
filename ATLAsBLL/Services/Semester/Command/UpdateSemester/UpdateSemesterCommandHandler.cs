using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Semesters.Commands.UpdateSemester
{
    public class UpdateSemesterCommandHandler : IRequestHandler<UpdateSemesterCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateSemesterCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

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