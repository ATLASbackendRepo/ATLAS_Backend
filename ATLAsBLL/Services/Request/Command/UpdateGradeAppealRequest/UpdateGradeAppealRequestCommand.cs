
using ATLASDomain.DTO.Request;
using MediatR;

namespace ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest
{

    public record UpdateGradeAppealRequestCommand(
        int RequestId,
        int DoctorUserId,
        UpdateGradeAppealRequestDto Request)
        : ICommand<bool>;
}
