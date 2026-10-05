using ATLASDomain.DTO.Staff;
using MediatR;

namespace ATLAsBLL.Features.Users.Commands.UpdateUser
{
    /// <summary>
    /// Update User Command.
    /// </summary>
    public record UpdateUserCommand(Create_Update_StaffDto Update_StaffDto) : ICommand<bool>, IWriteCommand;
}
