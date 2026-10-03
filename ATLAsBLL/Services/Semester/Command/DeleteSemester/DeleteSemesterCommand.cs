using MediatR;

namespace ATLAsBLL.Features.Semesters.Commands.DeleteSemester
{
    /// <summary>
    /// Delete Semester Command.
    /// </summary>
    public record DeleteSemesterCommand(int SemesterID) : ICommand<bool>, IWriteCommand;
}
