using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ATLAsBLL.Features.AssessmentModules.Commands.DeleteAssessmentModule
{
    public record DeleteAssessmentModuleCommand(int ModuleId, int UserId) : IRequest<bool>;
}
