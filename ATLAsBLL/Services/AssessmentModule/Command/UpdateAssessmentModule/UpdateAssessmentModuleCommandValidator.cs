using ATLAsBLL.Features.AssessmentModules.Commands.UpdateAssessmentModule;
using FluentValidation;
using ATLASDomain.Entities;

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
                .InclusiveBetween(AssessmentModule.MinimumMaxDegree, AssessmentModule.MaximumMaxDegree)
                .WithMessage("Max degree must be a value between 1 and 100.");
        }
    }
}
