using FluentValidation;

namespace ATLAsBLL.Features.CourseGrades.Commands.DeleteCourseGrade
{
    /// <summary>
    /// Delete Course Grade Command Validator.
    /// </summary>
    public class DeleteCourseGradeCommandValidator : AbstractValidator<DeleteCourseGradeCommand>
    {
        public DeleteCourseGradeCommandValidator()
        {
            RuleFor(g => g.CG_ID).NotEmpty().GreaterThan(0).WithMessage("Invalid Course Grade ID for deletion.");
        }
    }
}
