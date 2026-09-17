using ATLASDomain.DTO.Course;
using MediatR;

namespace ATLAsBLL.Features.Courses.Commands.CreateCourse
{
    public record CreateCourseCommand(CreateCourseDTO CreateCourseDTO) : ICommand<int>;
}