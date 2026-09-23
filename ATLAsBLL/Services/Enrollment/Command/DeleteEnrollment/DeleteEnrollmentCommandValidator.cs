using FluentValidation;

namespace ATLAsBLL.Features.Enrollments.Commands.DeleteEnrollment
{
    /// <summary>
    /// Delete Enrollment Command Validator.
    /// </summary>
    public class DeleteEnrollmentCommandValidator : AbstractValidator<DeleteEnrollmentCommand>
    {
        public DeleteEnrollmentCommandValidator()
        {
            RuleFor(e => e.EnrollmentID)
                .NotEmpty().GreaterThan(0).WithMessage("Invalid Enrollment ID.");
        }
    }
}
