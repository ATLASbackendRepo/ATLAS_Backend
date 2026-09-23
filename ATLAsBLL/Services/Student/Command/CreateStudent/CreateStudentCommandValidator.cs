using FluentValidation;

namespace ATLAsBLL.Features.Students.Commands.CreateStudent
{
    /// <summary>
    /// Create Student Command Validator.
    /// </summary>
    public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
    {
        public CreateStudentCommandValidator()
        {
          
            RuleFor(s => s.CreateStudentDTO.FullName).NotEmpty().WithMessage("Full name is required.");
            RuleFor(s => s.CreateStudentDTO.AcademicId).NotEmpty().WithMessage("Academic ID is required.");

        
            RuleFor(s => s.CreateStudentDTO.AcademicLevel).GreaterThan(0).WithMessage("Academic level is required.");
            RuleFor(s => s.CreateStudentDTO.DepartmentId).GreaterThan(0).WithMessage("Invalid Department ID.");
        }
    }
}
