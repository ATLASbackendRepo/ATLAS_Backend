using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Department.Comand.UpdateDepartment
{
    public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDepartmentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(request.DepartmentID);

            if (department == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Department with ID {request.DepartmentID} was not found.");
            }


            department.D_Name = request.DepartmentName;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}