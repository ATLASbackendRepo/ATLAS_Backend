using ATLASDomain.DTO.AssessmentModule;
using MediatR;

namespace ATLAsBLL.Features.AssessmentModules.Commands.CreateAssessmentModule
{
    public record CreateAssessmentModuleCommand(CreateModeDTO DTO, int UserId) : ICommand<int>;
}

