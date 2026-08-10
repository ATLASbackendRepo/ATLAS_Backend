using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.AssessmentModules.Commands.DeleteAssessmentModule
{
    public class DeleteAssessmentModuleCommandHandler : IRequestHandler<DeleteAssessmentModuleCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteAssessmentModuleCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<bool> Handle(DeleteAssessmentModuleCommand request, CancellationToken cancellationToken)
        {
            var module = await _unitOfWork.AssessmentModules.GetByIdAsync(request.ModuleId);
            if (module == null) throw new System.Collections.Generic.KeyNotFoundException("Module not found.");

            _unitOfWork.AssessmentModules.Delete(module);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}