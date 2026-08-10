using MediatR;

namespace ATLAsBLL.Features.CourseGrades.Commands.DeleteCourseGrade
{
    public record DeleteCourseGradeCommand(int CG_ID) : ICommand<bool>;
}