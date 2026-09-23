using ATLASDomain.DTO.Course;
using MediatR;

namespace ATLAsBLL.Features.Courses.Commands.CreateCourse
{
    /// <summary>
    /// Create Course Command.
    /// </summary>
    public record CreateCourseCommand(CreateCourseDTO CreateCourseDTO) : ICommand<int>;
}
