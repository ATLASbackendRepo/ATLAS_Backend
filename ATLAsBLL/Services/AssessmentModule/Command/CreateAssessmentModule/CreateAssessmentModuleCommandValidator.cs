using ATLAsBLL.Features.AssessmentModules.Commands.CreateAssessmentModule;
using FluentValidation;
using AssessmentModuleEntity = ATLASDomain.Entities.AssessmentModule;

namespace ATLAsBLL.Services.AssessmentModule.Command.CreateAssessmentModule
{
    /// <summary>
    /// Create Assessment Module Command Validator.
    /// </summary>
    public class CreateAssessmentModuleCommandValidator : AbstractValidator<CreateAssessmentModuleCommand>
    {
      public CreateAssessmentModuleCommandValidator()
        {
            RuleFor(c => c.DTO.ModuleName)
                .NotEmpty().WithMessage("Module name is required.")
                .MaximumLength(150).WithMessage("Module name cannot exceed 150 characters.");
            RuleFor(c => c.DTO.MaxDegree)
                .NotEmpty().WithMessage("Max degree is required.")
                .InclusiveBetween(AssessmentModuleEntity.MinimumMaxDegree, AssessmentModuleEntity.MaximumMaxDegree)
                .WithMessage("Max degree must be a value between 1 and 100.");
        }
    }
}
