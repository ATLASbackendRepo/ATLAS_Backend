using MediatR;

namespace ATLAsBLL.Features.Users.Commands.DeleteUser
{
    /// <summary>
    /// Delete User Command.
    /// </summary>
    public record DeleteUserCommand(string  AcademicId) : ICommand<bool>, IWriteCommand;
}
