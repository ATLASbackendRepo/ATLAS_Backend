
using ATLASDomain.DTO.Request;
using MediatR;

namespace ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest
{

    /// <summary>
    /// Update Grade Appeal Request Command.
    /// </summary>
    public record UpdateGradeAppealRequestCommand(
        int RequestId,
        int DoctorUserId,
        UpdateGradeAppealRequestDto Request)
        : ICommand<bool>;
}
