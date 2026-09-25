using ATLAsBLL.Features.Students.Commands.UpdateStudent;
using FluentValidation;

namespace ATLAsBLL.Services.Student.Command.UpdateStudent
{
    /// <summary>
    /// Update Student Command Validator.
    /// </summary>
    public class UpdateStudentCommandValidator
        : AbstractValidator<UpdateStudentCommand>
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="UpdateStudentCommandValidator"/> class.
        /// </summary>
        public UpdateStudentCommandValidator()
        {
            RuleFor(command => command.UpdateStudentDto)
                .NotNull()
                .WithMessage("Student data is required.");

            RuleFor(command => command.UpdateStudentDto.StudentCode)
                .NotEmpty()
                .WithMessage("Student code is required.");

            RuleFor(command => command.UpdateStudentDto.FullName)
                .MaximumLength(200)
                .WithMessage("Full name cannot exceed 200 characters.");

            RuleFor(command => command.UpdateStudentDto.AcademicLevel)
                .GreaterThan(0)
                .When(command => command.UpdateStudentDto.AcademicLevel.HasValue)
                .WithMessage("Academic level must be greater than zero.");

            RuleFor(command => command.UpdateStudentDto.DepartmentId)
                .GreaterThan(0)
                .When(command => command.UpdateStudentDto.DepartmentId.HasValue)
                .WithMessage("Invalid Department ID.");
        }
    }
}
