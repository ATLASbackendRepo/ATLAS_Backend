using FluentValidation;

namespace ATLAsBLL.Features.CourseOfferings.Commands.UpdateCourseOffering
{
    public class UpdateCourseOfferingCommandValidator : AbstractValidator<UpdateCourseOfferingCommand>
    {
        public UpdateCourseOfferingCommandValidator()
        {
            RuleFor(x => x.CourseOfferingId).NotEmpty().GreaterThan(0).WithMessage("Invalid Course Offering ID.");
            RuleFor(x => x.CourseId).NotEmpty().GreaterThan(0).WithMessage("Invalid Course ID.");
            RuleFor(x => x.SemesterId).NotEmpty().GreaterThan(0).WithMessage("Invalid Semester ID.");
        }
    }
}