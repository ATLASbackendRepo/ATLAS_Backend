using FluentValidation;

namespace ATLAsBLL.Features.Semesters.Commands.UpdateSemester
{
    public class UpdateSemesterCommandValidator : AbstractValidator<UpdateSemesterCommand>
    {
        public UpdateSemesterCommandValidator()
        {
            RuleFor(s => s.SemesterID)
                .NotEmpty().WithMessage("Semester ID is required.")
                .GreaterThan(0).WithMessage("Invalid Semester ID.");

            RuleFor(s => s.SemesterName)
                .NotEmpty().WithMessage("Semester name is required.")
                .MaximumLength(50).WithMessage("Semester name cannot exceed 50 characters.");

            RuleFor(s => s.AcademicYear)
                .NotEmpty().WithMessage("Academic year is required.")
                .MaximumLength(20).WithMessage("Academic year cannot exceed 20 characters.");
        }
    }
}