using ATLASDomain.DTO.Profile;
using MediatR;

namespace ATLAsBLL.Services.profile.Student.Query
{
    /// <summary>
    /// Get Student Profile By User Id Query.
    /// </summary>
    public class GetStudentProfileByUserIdQuery : IRequest<StudentProfileResponseDTO?>
    {
        /// <summary>
        /// User Id.
        /// </summary>
        public int UserId { get; set; }

        public GetStudentProfileByUserIdQuery(int userId)
        {
            UserId = userId;
        }
    }
}
