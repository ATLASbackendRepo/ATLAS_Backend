using FluentValidation;

namespace ATLAsBLL.Features.Department.Comand.UpdateDepartment
{
    /// <summary>
    /// Update Department Command Validator.
    /// </summary>
    public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
    {
        public UpdateDepartmentCommandValidator()
        {
            RuleFor(d => d.DepartmentID)
                .NotEmpty().WithMessage("Department ID is required.")
                .GreaterThan(0).WithMessage("Invalid Department ID.");

            RuleFor(d => d.DepartmentName)
                .NotEmpty().WithMessage("Department name is required.")
                .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");
        }
    }
}
