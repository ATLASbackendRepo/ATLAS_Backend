using MediatR;

namespace ATLAsBLL.Features.Users.Commands.DeleteUser
{
    public record DeleteUserCommand(string  AcademicId) : ICommand<bool>;
}