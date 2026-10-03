using ATLASDomain.DTO.AssessmentModule;
using MediatR;

namespace ATLAsBLL.Features.AssessmentModules.Commands.CreateAssessmentModule
{
    /// <summary>
    /// Create Assessment Module Command.
    /// </summary>
    public record CreateAssessmentModuleCommand(CreateModeDTO DTO, int UserId) : ICommand<int>, IWriteCommand;
}
