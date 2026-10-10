using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Users.Commands.UpdateUser
{
    /// <summary>
    /// Update User Command Handler.
    /// </summary>
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetUserByAcademicIDAsync(request.Update_StaffDto.AcademicId, cancellationToken);

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
            bool institutionChanged = false;
            if (request.Update_StaffDto.InstituteId != default(int))
            {
                if (user.InstitutionID != request.Update_StaffDto.InstituteId)
                {
                    institutionChanged = true;
                    user.InstitutionID = request.Update_StaffDto.InstituteId;
                }
            }   
            bool roleChanged = false;
            if (request.Update_StaffDto.Role != 0)
            {
                if (user.Role != request.Update_StaffDto.Role)
                {
                    roleChanged = true;
                }
                user.Role = request.Update_StaffDto.Role;
            }

            bool needsSecurityStampBump = roleChanged || institutionChanged;
            if (needsSecurityStampBump)
            {
                user.SecurityStamp = System.Guid.NewGuid();
            }

            var saved = await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
            if (needsSecurityStampBump)
            {
                await _unitOfWork.Users.BumpSecurityStampAsync(user.UserId, cancellationToken);
            }
            return true;
        }
    }
}
