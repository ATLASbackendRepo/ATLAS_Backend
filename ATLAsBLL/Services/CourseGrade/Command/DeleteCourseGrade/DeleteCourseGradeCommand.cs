using MediatR;

namespace ATLAsBLL.Features.CourseGrades.Commands.DeleteCourseGrade
{
    /// <summary>
    /// Delete Course Grade Command.
    /// </summary>
    public record DeleteCourseGradeCommand(int CG_ID) : ICommand<bool>, IWriteCommand;
}
