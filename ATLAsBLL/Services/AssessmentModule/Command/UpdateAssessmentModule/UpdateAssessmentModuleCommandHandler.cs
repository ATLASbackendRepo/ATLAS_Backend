using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.AssessmentModules.Commands.UpdateAssessmentModule
{
    public class UpdateAssessmentModuleCommandHandler : IRequestHandler<UpdateAssessmentModuleCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateAssessmentModuleCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<bool> Handle(UpdateAssessmentModuleCommand request, CancellationToken cancellationToken)
        {
            var module = await _unitOfWork.AssessmentModules.GetByIdAsync(request.ModuleId);
            if (module == null) throw new System.Collections.Generic.KeyNotFoundException("Module not found.");

            module.ModuleName = request.ModuleName;
            module.MaxDegree = request.MaxDegree;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}