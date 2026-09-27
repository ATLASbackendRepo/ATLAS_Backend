using MediatR;

namespace ATLAsBLL.Features.Semesters.Commands.UpdateSemester
{
    /// <summary>
    /// Update Semester Command.
    /// </summary>
    public record UpdateSemesterCommand(int SemesterID, string SemesterName, string AcademicYear, bool IsGradingClosed) : ICommand<bool>;
}
