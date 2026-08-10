using FluentValidation;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange
{
    public class CreateEnrollmentsRangeCommandValidator : AbstractValidator<CreateEnrollmentsRangeCommand>
    {
        public CreateEnrollmentsRangeCommandValidator()
        {
            RuleFor(x => x.Enrollments).NotEmpty().WithMessage("Enrollments list cannot be empty.");
            RuleFor(x => x.Enrollments)
                .Must(enrollments => enrollments
                    .Select(e => (e.StudentId, e.CourseOfferingId))
                    .Distinct()
                    .Count() == enrollments.Count)
                .WithMessage("An enrollment batch cannot contain the same student and course offering more than once.");

            RuleForEach(x => x.Enrollments).ChildRules(item =>
            {
                item.RuleFor(e => e.StudentId).NotEmpty().GreaterThan(0).WithMessage("Invalid Student ID.");
                item.RuleFor(e => e.CourseOfferingId).NotEmpty().GreaterThan(0).WithMessage("Invalid Course Offering ID.");
            });
        }
    }
}
