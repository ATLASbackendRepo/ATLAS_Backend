using MediatR;

namespace ATLAsBLL.Features.CourseGrades.Commands.UpdateCourseGrade
{
    /// <summary>
    /// Assign Course Grade Command.
    /// </summary>
    public record AssignCourseGradeCommand(
            int EnrollmentId,
        int ModuleId,
        decimal StudentDegree,
        int UserId
    ) : ICommand<bool>;
}
