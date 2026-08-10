using FluentValidation;

namespace ATLAsBLL.Features.Department.Comand.DeleteDepartment
{
    public class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
    {
        public DeleteDepartmentCommandValidator()
        {
            RuleFor(d => d.DepartmentID)
                .NotEmpty().WithMessage("Department ID is required for deletion.")
                .GreaterThan(0).WithMessage("Invalid Department ID.");
        }
    }
}