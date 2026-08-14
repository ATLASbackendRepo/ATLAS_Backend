using ATLASDomain.DTO.Request;
using MediatR;

namespace ATLAsBLL.Services.Request.Qeuries
{

    public record GetGradeAppealByIdQuery(
        int RequestId,
        int CurrentUserId,
        int? CurrentStudentId,
        bool IsSuperAdmin)
        : IQuery<GradeAppealRequestDto?>;
}
