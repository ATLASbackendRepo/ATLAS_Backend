using MediatR;

namespace ATLAsBLL.Features.Users.Commands.UpdateUser
{
    public record UpdateUserCommand(int UserId, string FullName) : ICommand<bool>;
}