
using ATLASDomain.DTO.Request;
using MediatR;
namespace ATLAsBLL.Services.Request.Qeuries
{

    /// <summary>
    /// Get Student Grade Appeals Query.
    /// </summary>
    public record GetStudentGradeAppealsQuery(int UserId)
        : IQuery<List<GradeAppealRequestDto>>;
}
