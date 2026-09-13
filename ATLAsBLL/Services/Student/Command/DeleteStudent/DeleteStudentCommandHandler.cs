using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Students.Commands.DeleteStudent
{
    public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteStudentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
        {
            
            var student = await _unitOfWork.Students.GetStudentWithUserAsync(request.StudentCode);

            if (student == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Student with ID {request.StudentCode} does not exist.");
            }

            var user = student.User;

            
            student.IsActive = false; 

            
            if (user != null)
            {
               user.IsActive = false; 
            }

           
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}