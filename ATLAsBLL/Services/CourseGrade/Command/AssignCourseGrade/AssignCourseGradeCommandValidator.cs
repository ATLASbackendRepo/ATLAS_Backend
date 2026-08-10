using FluentValidation;

namespace ATLAsBLL.Features.CourseGrades.Commands.UpdateCourseGrade
{
    public class AssignCourseGradeCommandValidator : AbstractValidator<AssignCourseGradeCommand>
    {
        public AssignCourseGradeCommandValidator()
        {
            RuleFor(g => g.EnrollmentId).NotEmpty().GreaterThan(0).WithMessage("Invalid Enrollment ID.");
            RuleFor(g => g.ModuleId).NotEmpty().GreaterThan(0).WithMessage("Invalid Module ID.");
            RuleFor(g => g.StudentDegree).InclusiveBetween(0, 100).WithMessage("Degree must be between 0 and 100.");
            RuleFor(g => g.UserId).NotEmpty().GreaterThan(0).WithMessage("UpdatedBy User ID is required.");
        }
    }
}