// ATLAsBLL/Features/StaffProfiles/Queries/GetStaffProfileByUserId/GetStaffProfileByUserIdQuery.cs
using ATLASDomain.DTO.Profile;
using MediatR;

namespace ATLAsBLL.Services.profile.Staff.Query
{
    public class GetStaffProfileByUserIdQuery : IRequest<StaffProfileResponseDto?>
    {
        public int UserId { get; set; }

        public GetStaffProfileByUserIdQuery(int userId)
        {
            UserId = userId;
        }
    }
}