using FluentValidation;

namespace ATLAsBLL.Features.Department.Comand.CreateDepartment
{
    public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentCommandValidator()
        {
            RuleFor(d => d.DepartmentName)
                .NotEmpty().WithMessage("Department name is required.")
                .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");
            RuleFor(d => d.InstitutionId)
                .GreaterThan(0).WithMessage("Institution ID must be greater than 0.");
        }
    }
}