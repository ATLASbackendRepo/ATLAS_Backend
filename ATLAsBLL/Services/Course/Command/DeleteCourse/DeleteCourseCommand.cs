using MediatR;

namespace ATLAsBLL.Features.Courses.Commands.DeleteCourse
{
    public record DeleteCourseCommand(int CourseID) : ICommand<bool>;
}