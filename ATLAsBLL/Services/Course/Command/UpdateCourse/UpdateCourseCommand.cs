using MediatR;

namespace ATLAsBLL.Features.Courses.Commands.UpdateCourse
{
    public record UpdateCourseCommand(int CourseID, string CourseName, int CreditHours, bool IsPractical) : ICommand<bool>;
}