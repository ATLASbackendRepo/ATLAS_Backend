using ATLASDomain.DTO.Request;
using MediatR;

namespace ATLAsBLL.Services.Request.Qeuries
{

    /// <summary>
    /// Get Grade Appeal By Id Query.
    /// </summary>
    public record GetGradeAppealByIdQuery(
        int RequestId,
        int CurrentUserId,
        int? CurrentStudentId,
        bool IsSuperAdmin)
        : IQuery<GradeAppealRequestDto?>;
}
