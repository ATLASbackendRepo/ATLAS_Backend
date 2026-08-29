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
    public class updateStaffProfileInfoHandler : IRequestHandler<updateStaffProfileInfoCommand , bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public updateStaffProfileInfoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(
            updateStaffProfileInfoCommand request,
            CancellationToken cancellationToken)
        {
            var profile = await _unitOfWork.staffProfile.GetByUserIdAsync(request.UserId);

            if (profile == null || profile.User == null || request.StaffProfileUpdateDTO.FullName
                
                == null)
                return false;

            // Update the profile with the new information
            profile.User.FullName = request.StaffProfileUpdateDTO.FullName;
            profile.DepartmentID = request.StaffProfileUpdateDTO.DepartmentID;
            profile.TeachingYears = request.StaffProfileUpdateDTO.TeachingYears;

             _unitOfWork.staffProfile.Update(profile);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}       