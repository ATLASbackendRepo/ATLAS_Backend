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
            
            var module = await _unitOfWork.AssessmentModules.GetByIdAsync(request.DTO.ModuleId);
            if (module == null) throw new System.Collections.Generic.KeyNotFoundException("Module not found.");
            if (request.DTO.MaxDegree < 0 || request.DTO.MaxDegree > 100)
            {
                throw new ArgumentException("MaxDegree must be a value between 0 and 100.");
            }
            if (string.IsNullOrWhiteSpace(request.DTO.ModuleName))
            {
                throw new ArgumentException("ModuleName cannot be null or empty.");
            }



            module.ModuleName = request.DTO.ModuleName;
            module.MaxDegree = request.DTO.MaxDegree;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}