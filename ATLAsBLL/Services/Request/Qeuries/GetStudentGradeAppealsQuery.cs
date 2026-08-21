
using ATLASDomain.DTO.Request;
using MediatR;
namespace ATLAsBLL.Services.Request.Qeuries
{

    public record GetStudentGradeAppealsQuery(int UserId)
        : IQuery<List<GradeAppealRequestDto>>;
}
