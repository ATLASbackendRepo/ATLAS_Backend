using ATLAsBLL.Services.profile.Staff.Query;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Profile;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.profile.Staff.command
{
    /// <summary>
    /// update Staff Profile Info Handler.
    /// </summary>
    public class updateStaffProfileInfoHandler : IRequestHandler<updateStaffProfileInfoCommand , bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public updateStaffProfileInfoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(
            updateStaffProfileInfoCommand request,
            CancellationToken cancellationToken)
        {
            var profile = await _unitOfWork.StaffProfile.GetByUserIdAsync(request.UserId, cancellationToken);

            if (profile == null || profile.User == null)
                return false;

            // Self-service updates edit only non-restricted profile fields (e.g. TeachingYears).
            // Administrative fields like FullName and DepartmentID cannot be modified here.
            profile.TeachingYears = request.StaffProfileUpdateDTO.TeachingYears;
            profile.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}       
