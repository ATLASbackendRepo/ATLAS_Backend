using MediatR;

namespace ATLAsBLL.Features.CourseGrades.Commands.UpdateCourseGrade
{
    public record AssignCourseGradeCommand(
            int EnrollmentId,
        int ModuleId,
        decimal StudentDegree,
        int UserId
    ) : ICommand<bool>;
}