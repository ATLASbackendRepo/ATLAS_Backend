using FluentValidation;

namespace ATLAsBLL.Features.Students.Commands.DeleteStudent
{
    public class DeleteStudentCommandValidator : AbstractValidator<DeleteStudentCommand>
    {
        public DeleteStudentCommandValidator()
        {
            RuleFor(s => s.StudentId)
                .NotEmpty().WithMessage("Student ID is required to perform deletion.")
                .GreaterThan(0).WithMessage("Invalid Student ID.");
        }
    }
}