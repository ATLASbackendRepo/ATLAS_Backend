using ATLASDomain.DTO.Profile;
using MediatR;

namespace ATLAsBLL.Services.profile.Student.Query
{
    public class GetStudentProfileByUserIdQuery : IRequest<StudentProfileResponseDTO?>
    {
        public int UserId { get; set; }

        public GetStudentProfileByUserIdQuery(int userId)
        {
            UserId = userId;
        }
    }
}
