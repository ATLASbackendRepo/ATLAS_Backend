using MediatR;

namespace ATLAsBLL.Features.Users.Commands.DeleteUser
{
    public record DeleteUserCommand(int UserId) : ICommand<bool>;
}