using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Students.Commands.UpdateStudent
{
    public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateStudentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
        {
            
            var student = await _unitOfWork.Students.GetStudentWithUserAsync(request.UpdateStudentDto.StudentCode);

            if (student == null || student.User == null)
            {
                    throw new System.Collections.Generic.KeyNotFoundException($"Student with ID {request.UpdateStudentDto.StudentCode} was not found.");
                
            }

            if(!string.IsNullOrEmpty(request.UpdateStudentDto.FullName))
            {
                student.User.FullName = request.UpdateStudentDto.FullName;
            }
            if(request.UpdateStudentDto.AcademicLevel.HasValue)
            {
                student.AcademicLevel = request.UpdateStudentDto.AcademicLevel.Value;
            }
            if(request.UpdateStudentDto.DepartmentId.HasValue)
            {
                student.DepartmentId = request.UpdateStudentDto.DepartmentId.Value;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}