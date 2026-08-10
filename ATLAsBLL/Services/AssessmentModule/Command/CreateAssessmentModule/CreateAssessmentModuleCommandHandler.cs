using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.AssessmentModules.Commands.CreateAssessmentModule
{
    public class CreateAssessmentModuleCommandHandler : IRequestHandler<CreateAssessmentModuleCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateAssessmentModuleCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<int> Handle(CreateAssessmentModuleCommand request, CancellationToken cancellationToken)
        {
            var module = new AssessmentModule
            {
                CourseOfferingID = request.CourseOfferingId,
                ModuleName = request.ModuleName,
                MaxDegree = request.MaxDegree
            };
            await _unitOfWork.AssessmentModules.AddAsync(module);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return module.ModuleID;
        }
    }
}
