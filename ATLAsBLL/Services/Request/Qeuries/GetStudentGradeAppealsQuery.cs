
using ATLASDomain.DTO.Request;
using MediatR;
namespace ATLAsBLL.Services.Request.Qeuries
{

    public record GetStudentGradeAppealsQuery(int StudentId)
        : IQuery<List<GradeAppealRequestDto>>;
}
