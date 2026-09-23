using ATLASDomain.DTO.Profile;
using MediatR;

namespace ATLAsBLL.Services.profile.Staff.command
{
    /// <summary>
    /// update Staff Profile Info Command.
    /// </summary>
    public class updateStaffProfileInfoCommand : IRequest<bool>
    {
        /// <summary>
        /// User Id.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Staff Profile Update DTO.
        /// </summary>
        public StaffProfileUpdateDTO StaffProfileUpdateDTO { get; set; }

        public updateStaffProfileInfoCommand(int userId, StaffProfileUpdateDTO staffProfileUpdateDTO)
        {
            UserId = userId;
            StaffProfileUpdateDTO = staffProfileUpdateDTO;
        }
    }
}
