using FluentValidation;

namespace ATLAsBLL.Features.Courses.Commands.DeleteCourse
{
    /// <summary>
    /// Delete Course Command Validator.
    /// </summary>
    public class DeleteCourseCommandValidator : AbstractValidator<DeleteCourseCommand>
    {
        public DeleteCourseCommandValidator()
        {
            RuleFor(c => c.CourseID)
                .NotEmpty().WithMessage("Course ID is required to perform deletion.")
                .GreaterThan(0).WithMessage("Invalid Course ID.");
        }
    }
}
