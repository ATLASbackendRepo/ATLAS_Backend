using MediatR;

namespace ATLAsBLL.Features.Courses.Commands.CreateCourse
{
    public record CreateCourseCommand(string CourseName, int CreditHours, bool IsPractical) : ICommand<int>;
}