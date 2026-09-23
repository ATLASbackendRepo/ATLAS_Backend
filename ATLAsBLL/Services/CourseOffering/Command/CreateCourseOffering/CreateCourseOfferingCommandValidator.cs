using FluentValidation;

namespace ATLAsBLL.Features.CourseOfferings.Commands.CreateCourseOffering
{
    /// <summary>
    /// Create Course Offering Command Validator.
    /// </summary>
    public class CreateCourseOfferingCommandValidator : AbstractValidator<CreateCourseOfferingCommand>
    {
        public CreateCourseOfferingCommandValidator()
        {
            RuleFor(x => x.CourseId).NotEmpty().GreaterThan(0).WithMessage("Invalid Course ID.");
            RuleFor(x => x.SemesterId).NotEmpty().GreaterThan(0).WithMessage("Invalid Semester ID.");
        }
    }
}
