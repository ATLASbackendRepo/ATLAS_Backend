using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Semesters.Commands.CreateSemester
{
    public class CreateSemesterCommandHandler : IRequestHandler<CreateSemesterCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateSemesterCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

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