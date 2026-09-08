using ATLAsBLL.Features.AssessmentModules.Commands.UpdateAssessmentModule;
using FluentValidation;

namespace ATLAsBLL.Services.AssessmentModule.Command.UpdateAssessmentModule
{
    internal class UpdateAssessmentModuleCommandValidator : AbstractValidator<UpdateAssessmentModuleCommand>
    {
        public UpdateAssessmentModuleCommandValidator()
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
