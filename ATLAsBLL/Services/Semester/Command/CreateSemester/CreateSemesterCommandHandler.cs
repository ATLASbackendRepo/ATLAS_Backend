using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Semesters.Commands.CreateSemester
{
    /// <summary>
    /// Create Semester Command Handler.
    /// </summary>
    public class CreateSemesterCommandHandler : IRequestHandler<CreateSemesterCommand, int>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public CreateSemesterCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<int> Handle(CreateSemesterCommand request, CancellationToken cancellationToken)
        {
            var semester = new Semester
            {
                SemesterName = request.SemesterName,
                AcademicYear = request.AcademicYear
            };

            await _unitOfWork.Semesters.AddAsync(semester);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return semester.SemesterID;
        }
    }
}
