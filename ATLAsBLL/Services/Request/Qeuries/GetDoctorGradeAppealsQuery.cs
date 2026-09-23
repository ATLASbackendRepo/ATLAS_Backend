

using ATLASDomain.DTO.Request;
using MediatR;
namespace ATLAsBLL.Services.Request.Qeuries
{

    /// <summary>
    /// Get Doctor Grade Appeals Query.
    /// </summary>
    public record GetDoctorGradeAppealsQuery(int DoctorUserId)
        : IQuery<List<GradeAppealRequestDto>>;
}
