using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Profile;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.profile.Staff.Query
{

    /// <summary>
    /// Get Staff Profile By User Id Query Handler.
    /// </summary>
    public class GetStaffProfileByUserIdQueryHandler
        : IRequestHandler<GetStaffProfileByUserIdQuery, StaffProfileResponseDto?>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public GetStaffProfileByUserIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<StaffProfileResponseDto?> Handle(
            GetStaffProfileByUserIdQuery request,
            CancellationToken cancellationToken)
        {
            var profile = await _unitOfWork.StaffProfile.GetByUserIdAsync(request.UserId);

            if (profile == null|| profile.User == null || profile.Role == null)
                return null;

            return new StaffProfileResponseDto
            {
                StaffProfileID = profile.StaffProfileID,
                AcademicId = profile.AcademicId ?? string.Empty,
                FullName = profile.User.FullName ?? string.Empty, 
                Role = profile.Role.ToString(),
                DepartmentID = profile.DepartmentID,
                TeachingYears = profile.TeachingYears,
             
            };
        }
    }
}
