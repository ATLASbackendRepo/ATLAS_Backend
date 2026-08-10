using FluentValidation;

namespace ATLAsBLL.Features.Courses.Commands.UpdateCourse
{
    public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
    {
        public UpdateCourseCommandValidator()
        {
            RuleFor(c => c.CourseID)
                .NotEmpty().WithMessage("Course ID is required.")
                .GreaterThan(0).WithMessage("Course ID must be greater than 0.");

            RuleFor(c => c.CourseName)
                .NotEmpty().WithMessage("Course name is required.")
                .MaximumLength(150).WithMessage("Course name cannot exceed 150 characters.");

            RuleFor(c => c.CreditHours)
                .NotEmpty().WithMessage("Credit hours are required.")
                .GreaterThan(0).WithMessage("Credit hours must be greater than 0.");
        }
    }
}