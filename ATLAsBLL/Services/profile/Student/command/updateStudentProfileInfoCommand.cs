using ATLASDomain.DTO.Profile;
using MediatR;
namespace ATLAsBLL.Services.profile.Student.command
{
    public class updateStudentProfileInfoCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public StudentUpdateProfileInfoDTO StudentUpdateProfileInfoDTO { get; set; }

        public updateStudentProfileInfoCommand(int userId, StudentUpdateProfileInfoDTO studentUpdateProfileInfoDTO)
        {
            UserId = userId;
            StudentUpdateProfileInfoDTO = studentUpdateProfileInfoDTO;
        }
    }
}
