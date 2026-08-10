using FluentValidation;

namespace ATLAsBLL.Features.Courses.Commands.CreateCourse
{
    public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
    {
        public CreateCourseCommandValidator()
        {
            RuleFor(c => c.CourseName)
                .NotEmpty().WithMessage("Course name is required.")
                .MaximumLength(150).WithMessage("Course name cannot exceed 150 characters.");

            RuleFor(c => c.CreditHours)
                .NotEmpty().WithMessage("Credit hours are required.")
                .GreaterThan(0).WithMessage("Credit hours must be greater than 0.")
                .LessThanOrEqualTo(6).WithMessage("Credit hours cannot exceed 6 hours.");
        }
    }
}