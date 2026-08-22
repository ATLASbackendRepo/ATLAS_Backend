using ATLASDomain.DTO.Profile;
using MediatR;

namespace ATLAsBLL.Services.profile.Staff.command
{
    public class updateStaffProfileInfoCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public StaffProfileUpdateDTO StaffProfileUpdateDTO { get; set; }

        public updateStaffProfileInfoCommand(int userId, StaffProfileUpdateDTO staffProfileUpdateDTO)
        {
            UserId = userId;
            StaffProfileUpdateDTO = staffProfileUpdateDTO;
        }
    }
}
