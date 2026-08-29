using ATLASDomain.Entities;
using MediatR;


namespace ATLAsBLL.Features.Users.Commands.CreateUser
{
    public record CreateUserCommand(
        string FullName,
        string Password
        , string AcademicId
        , enUserRole Role
    ) : ICommand<int>;
}