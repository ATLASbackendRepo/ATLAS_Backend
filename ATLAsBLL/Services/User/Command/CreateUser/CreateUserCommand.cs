using ATLASDomain.Entities;
using ATLASDomain.DTO.Staff;
using ATLAsBLL.Services.Common;
using MediatR;

namespace ATLAsBLL.Features.Users.Commands.CreateUser
{
    /// <summary>
    /// Create User Command.
    /// </summary>
    public record CreateUserCommand(
       CreateStaffDto create_StaffDto
    ) : ICommand<CreateUserResult>, IWriteCommand;
}