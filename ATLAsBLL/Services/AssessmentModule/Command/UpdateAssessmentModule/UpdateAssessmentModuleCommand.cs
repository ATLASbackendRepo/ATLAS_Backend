using ATLASDomain.DTO.AssessmentModule;
using MediatR;

namespace ATLAsBLL.Features.AssessmentModules.Commands.UpdateAssessmentModule
{
    public record UpdateAssessmentModuleCommand(UpdateModeDTO DTO) : IRequest<bool>;
}
