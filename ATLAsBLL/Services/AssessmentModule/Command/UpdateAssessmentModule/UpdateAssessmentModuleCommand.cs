using MediatR;

namespace ATLAsBLL.Features.AssessmentModules.Commands.UpdateAssessmentModule
{
    public record UpdateAssessmentModuleCommand(int ModuleId, string ModuleName, decimal MaxDegree) : ICommand<bool>;
}
