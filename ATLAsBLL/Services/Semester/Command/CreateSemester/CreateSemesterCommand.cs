using MediatR;

namespace ATLAsBLL.Features.Semesters.Commands.CreateSemester
{
    public record CreateSemesterCommand(string SemesterName, string AcademicYear) : ICommand<int>;
}