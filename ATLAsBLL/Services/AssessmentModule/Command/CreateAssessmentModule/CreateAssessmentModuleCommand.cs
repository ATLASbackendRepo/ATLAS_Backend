using MediatR;

namespace ATLAsBLL.Features.AssessmentModules.Commands.CreateAssessmentModule
{
    public record CreateAssessmentModuleCommand(int CourseOfferingId, string ModuleName, decimal MaxDegree) : ICommand<int>;
}

