using FluentValidation;

namespace ATLAsBLL.Features.Semesters.Commands.DeleteSemester
{
    public class DeleteSemesterCommandValidator : AbstractValidator<DeleteSemesterCommand>
    {
        public DeleteSemesterCommandValidator()
        {
            RuleFor(s => s.SemesterID)
                .NotEmpty().WithMessage("Semester ID is required for deletion.")
                .GreaterThan(0).WithMessage("Invalid Semester ID.");
        }
    }
}