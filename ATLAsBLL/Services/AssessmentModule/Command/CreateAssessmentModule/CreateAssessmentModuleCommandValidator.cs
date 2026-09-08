using ATLAsBLL.Features.AssessmentModules.Commands.CreateAssessmentModule;
using FluentValidation;

namespace ATLAsBLL.Services.AssessmentModule.Command.CreateAssessmentModule
{
    public class CreateAssessmentModuleCommandValidator : AbstractValidator<CreateAssessmentModuleCommand>
    {
      public CreateAssessmentModuleCommandValidator()
        {
            RuleFor(c => c.DTO.ModuleName)
                .NotEmpty().WithMessage("Module name is required.")
                .MaximumLength(150).WithMessage("Module name cannot exceed 150 characters.");
            RuleFor(c => c.DTO.MaxDegree)
                .NotEmpty().WithMessage("Max degree is required.")
                .GreaterThan(0).WithMessage("Max degree must be greater than 0.");
        }
    }
}
