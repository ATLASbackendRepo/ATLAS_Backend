using MediatR;

namespace ATLAsBLL.Features.Semesters.Commands.CreateSemester
{
    /// <summary>
    /// Create Semester Command.
    /// </summary>
    public record CreateSemesterCommand(string SemesterName, string AcademicYear) : ICommand<int>;
}
