using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Students.Commands.DeleteStudent
{
    /// <summary>
    /// Delete Student Command Handler.
    /// </summary>
    public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public DeleteStudentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
        {
            
            var student = await _unitOfWork.Students.GetStudentWithUserAsync(request.StudentCode, cancellationToken);

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

           
            var affected = await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Report failure when nothing was written, for example when the
            // student was already deactivated.
            return affected > 0;
        }
    }
}
