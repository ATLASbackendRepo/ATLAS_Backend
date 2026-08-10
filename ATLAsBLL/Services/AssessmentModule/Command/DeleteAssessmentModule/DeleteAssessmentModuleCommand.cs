using MediatR;

namespace ATLAsBLL.Features.AssessmentModules.Commands.DeleteAssessmentModule
{
    public record DeleteAssessmentModuleCommand(int ModuleId) : ICommand<bool>;
}