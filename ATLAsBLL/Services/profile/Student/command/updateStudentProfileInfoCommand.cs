using ATLASDomain.DTO.Profile;
using MediatR;
namespace ATLAsBLL.Services.profile.Student.command
{
    /// <summary>
    /// update Student Profile Info Command.
    /// </summary>
    public class updateStudentProfileInfoCommand : IRequest<bool>
    {
        /// <summary>
        /// User Id.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Student Update Profile Info DTO.
        /// </summary>
        public StudentUpdateProfileInfoDTO StudentUpdateProfileInfoDTO { get; set; }

        public updateStudentProfileInfoCommand(int userId, StudentUpdateProfileInfoDTO studentUpdateProfileInfoDTO)
        {
            UserId = userId;
            StudentUpdateProfileInfoDTO = studentUpdateProfileInfoDTO;
        }
    }
}
