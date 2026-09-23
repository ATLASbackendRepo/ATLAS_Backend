using MediatR;

namespace ATLAsBLL.Features.Courses.Commands.DeleteCourse
{
    /// <summary>
    /// Delete Course Command.
    /// </summary>
    public record DeleteCourseCommand(int CourseID) : ICommand<bool>;
}
