using ATLASDomain.Entities;
using MediatR;


namespace ATLAsBLL.Features.Users.Commands.CreateUser
{
    public record CreateUserCommand(
        string FullName,
        string Password
        , enUserRole Role
    ) : ICommand<int>;
}