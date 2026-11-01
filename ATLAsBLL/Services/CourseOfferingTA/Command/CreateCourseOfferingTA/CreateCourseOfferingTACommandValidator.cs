using FluentValidation;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.CreateCourseOfferingTA
{
    /// <summary>
    /// Validator for CreateCourseOfferingTACommand.
    /// </summary>
    public class CreateCourseOfferingTACommandValidator : AbstractValidator<CreateCourseOfferingTACommand>
    {
        public CreateCourseOfferingTACommandValidator()
        {
            RuleFor(x => x.CourseOfferingId)
                .GreaterThan(0)
                .WithMessage("Course offering ID must be greater than zero.");

            RuleFor(x => x.TAID)
                .GreaterThan(0)
                .WithMessage("TA ID must be greater than zero.");
        }
    }
}

