using System;
using MediatR;
using System.Text;
using System.Threading.Tasks;
using ATLASDomain.DTO.Request;

namespace ATLAsBLL.Services.Request.Command.CreateGradeAppealRequest
{
    /// <summary>
    /// Create Grade Appeal Request Command.
    /// </summary>
    public record CreateGradeAppealRequestCommand(
      int UserId,
      CreateGradeAppealRequestDto Request)
      : ICommand<int>;
}
