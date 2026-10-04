using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Department.Comand.DeleteDepartment
{
    /// <summary>
    /// Delete Department Command Handler.
    /// </summary>
    public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public DeleteDepartmentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(request.DepartmentID, cancellationToken);

            if (department == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Department with ID {request.DepartmentID} does not exist.");
            }

            _unitOfWork.Departments.Delete(department);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
