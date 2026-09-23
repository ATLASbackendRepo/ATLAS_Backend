using FluentValidation;

namespace ATLAsBLL.Features.Courses.Commands.CreateCourse
{
    /// <summary>
    /// Create Course Command Validator.
    /// </summary>
    public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
    {
        public CreateCourseCommandValidator()
        {
            RuleFor(c => c.CreateCourseDTO.CourseName)
                .NotEmpty().WithMessage("Course name is required.")
                .MaximumLength(150).WithMessage("Course name cannot exceed 150 characters.");

            RuleFor(c => c.CreateCourseDTO.CreditHours)
                .NotEmpty().WithMessage("Credit hours are required.")
                .GreaterThan(0).WithMessage("Credit hours must be greater than 0.")
                .LessThanOrEqualTo(6).WithMessage("Credit hours cannot exceed 6 hours.");
        }
    }
}
