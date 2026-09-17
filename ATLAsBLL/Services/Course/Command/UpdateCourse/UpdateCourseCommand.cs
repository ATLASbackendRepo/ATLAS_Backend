using ATLASDomain.DTO.Course;
using MediatR;

namespace ATLAsBLL.Features.Courses.Commands.UpdateCourse
{
    public record UpdateCourseCommand(UpdateCourseDTO UpdateCourseDTO) : ICommand<bool>;
}