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
            var profile = await _unitOfWork.staffProfile.GetByUserIdAsync(request.UserId);

            if (profile == null || profile.User == null || request.StaffProfileUpdateDTO.FullName
                
                == null)
                return false;

            // The repository returns a tracked graph, so EF Core writes only the
            // properties changed here. Calling Update on the graph instead would
            // mark every column as modified and overwrite the user's password,
            // refresh-token hash and activation flag with stale values.
            profile.User.FullName = request.StaffProfileUpdateDTO.FullName;
            profile.DepartmentID = request.StaffProfileUpdateDTO.DepartmentID;
            profile.TeachingYears = request.StaffProfileUpdateDTO.TeachingYears;
            profile.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}       
