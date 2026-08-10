using FluentValidation;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment
{
    public class CreateEnrollmentCommandValidator : AbstractValidator<CreateEnrollmentCommand>
    {
        public CreateEnrollmentCommandValidator()
        {
            RuleFor(e => e.StudentId)
                .NotEmpty().WithMessage("Student ID is required.")
                .GreaterThan(0).WithMessage("Invalid Student ID.");

            RuleFor(e => e.CourseOfferingId)
                .NotEmpty().WithMessage("Course Offering ID is required.")
                .GreaterThan(0).WithMessage("Invalid Course Offering ID.");
        }
    }
}