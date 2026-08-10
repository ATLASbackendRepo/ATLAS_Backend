using MediatR;

namespace ATLAsBLL.Features.Semesters.Commands.UpdateSemester
{
    public record UpdateSemesterCommand(int SemesterID, string SemesterName, string AcademicYear) : ICommand<bool>;
}