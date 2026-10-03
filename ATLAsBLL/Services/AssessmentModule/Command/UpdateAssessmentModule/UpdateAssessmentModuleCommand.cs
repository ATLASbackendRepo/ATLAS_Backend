using ATLASDomain.DTO.AssessmentModule;
using MediatR;

namespace ATLAsBLL.Features.AssessmentModules.Commands.UpdateAssessmentModule
{
    /// <summary>
    /// Update Assessment Module Command.
    /// </summary>
    public record UpdateAssessmentModuleCommand(UpdateModeDTO DTO, int UserId) : ICommand<bool>, IWriteCommand;
}
