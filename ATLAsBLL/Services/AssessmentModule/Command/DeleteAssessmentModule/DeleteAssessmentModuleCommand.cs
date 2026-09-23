using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ATLAsBLL.Features.AssessmentModules.Commands.DeleteAssessmentModule
{
    /// <summary>
    /// Delete Assessment Module Command.
    /// </summary>
    public record DeleteAssessmentModuleCommand(int ModuleId, int UserId) : IRequest<bool>;
}
