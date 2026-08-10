using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Department.Comand.DeleteDepartment
{
    public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteDepartmentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(request.DepartmentID);

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