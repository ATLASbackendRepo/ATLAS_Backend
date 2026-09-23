using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Department.Comand.UpdateDepartment
{
    /// <summary>
    /// Update Department Command Handler.
    /// </summary>
    public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDepartmentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
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
