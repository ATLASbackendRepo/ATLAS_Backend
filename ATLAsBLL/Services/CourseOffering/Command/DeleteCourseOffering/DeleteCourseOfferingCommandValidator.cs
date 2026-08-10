using FluentValidation;

namespace ATLAsBLL.Features.CourseOfferings.Commands.DeleteCourseOffering
{
    public class DeleteCourseOfferingCommandValidator : AbstractValidator<DeleteCourseOfferingCommand>
    {
        public DeleteCourseOfferingCommandValidator()
        {
            RuleFor(x => x.CourseOfferingId).NotEmpty().GreaterThan(0).WithMessage("Invalid Course Offering ID for deletion.");
        }
    }
}
