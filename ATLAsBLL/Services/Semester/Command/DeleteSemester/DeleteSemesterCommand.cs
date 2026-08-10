using MediatR;

namespace ATLAsBLL.Features.Semesters.Commands.DeleteSemester
{
    public record DeleteSemesterCommand(int SemesterID) : ICommand<bool>;
}