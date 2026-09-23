using ATLASDomain.Entities;
using MediatR;
using ATLASDomain.DTO.Staff;


namespace ATLAsBLL.Features.Users.Commands.CreateUser
{
    /// <summary>
    /// Create User Command.
    /// </summary>
    public record CreateUserCommand(
       Create_Update_StaffDto create_StaffDto
    ) : ICommand<int>;
}
