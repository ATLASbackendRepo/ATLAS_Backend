using System;
using MediatR;
using System.Text;
using System.Threading.Tasks;
using ATLASDomain.DTO.Request;

namespace ATLAsBLL.Services.Request.Command.CreateGradeAppealRequest
{
    public record CreateGradeAppealRequestCommand(
      int UserId,
      CreateGradeAppealRequestDto Request)
      : ICommand<int>;
}
