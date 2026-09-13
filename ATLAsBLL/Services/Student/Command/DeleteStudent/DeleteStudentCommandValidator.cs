        using FluentValidation;

namespace ATLAsBLL.Features.Students.Commands.DeleteStudent
{
    public class DeleteStudentCommandValidator : AbstractValidator<DeleteStudentCommand>
    {
        public DeleteStudentCommandValidator()
        {
            RuleFor(s => s.StudentCode)
                .NotEmpty().WithMessage("Student Code is required to perform deletion.")
                .MaximumLength(50).WithMessage("Student Code cannot exceed 50 characters.");
        }
    }
}