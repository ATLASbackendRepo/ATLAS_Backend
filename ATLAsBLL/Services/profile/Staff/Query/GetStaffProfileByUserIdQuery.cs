// ATLAsBLL/Features/StaffProfiles/Queries/GetStaffProfileByUserId/GetStaffProfileByUserIdQuery.cs
using ATLASDomain.DTO.Profile;
using MediatR;

namespace ATLAsBLL.Services.profile.Staff.Query
{
    /// <summary>
    /// Get Staff Profile By User Id Query.
    /// </summary>
    public class GetStaffProfileByUserIdQuery : IRequest<StaffProfileResponseDto?>
    {
        /// <summary>
        /// User Id.
        /// </summary>
        public int UserId { get; set; }

        public GetStaffProfileByUserIdQuery(int userId)
        {
            UserId = userId;
        }
    }
}
