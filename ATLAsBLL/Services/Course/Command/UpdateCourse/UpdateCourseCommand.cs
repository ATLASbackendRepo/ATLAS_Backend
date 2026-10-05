using ATLASDomain.DTO.Course;
using MediatR;

namespace ATLAsBLL.Features.Courses.Commands.UpdateCourse
{
    /// <summary>
    /// Update Course Command.
    /// </summary>
    public record UpdateCourseCommand(UpdateCourseDTO UpdateCourseDTO) : ICommand<bool>, IWriteCommand;
}
