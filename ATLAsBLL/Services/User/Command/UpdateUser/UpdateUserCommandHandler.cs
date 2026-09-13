using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetUserByAcademicIDAsync(request.Update_StaffDto.AcademicId);

            if (user == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"User with ID {request.Update_StaffDto.AcademicId} was not found.");
            }
            if (!string.IsNullOrEmpty(request.Update_StaffDto.FullName))
                user.FullName = request.Update_StaffDto.FullName;

            if(request.Update_StaffDto.DepartmentId != default(int))
            {
                user.DepartmentId = request.Update_StaffDto.DepartmentId;
            }   
            if(request.Update_StaffDto.InstituteId != default(int))
            {
                user.InstitutionID = request.Update_StaffDto.InstituteId;
            }   
            user.Role = request.Update_StaffDto.Role;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}