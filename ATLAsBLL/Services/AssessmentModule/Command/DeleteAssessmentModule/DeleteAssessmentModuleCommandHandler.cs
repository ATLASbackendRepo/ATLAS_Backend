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
            if(request.ModuleId <= 0) throw new System.ArgumentException("Invalid module ID.");

            var module = await _unitOfWork.AssessmentModules.GetByIdAsync(request.ModuleId);
            if (module == null) throw new System.Collections.Generic.KeyNotFoundException("Module not found.");
            var doctors = await _unitOfWork.CourseOfferingDoctors.GetDoctorsByOfferingAsync(module.CourseOfferingID);
            var teachingAssistants = await _unitOfWork.CourseOfferingTAs.GetTAsByOfferingAsync(module.CourseOfferingID);
            if (!doctors.Contains(request.UserId) && !teachingAssistants.Any(ta => ta.TeacherAssistID == request.UserId))
                throw new UnauthorizedAccessException("You are not assigned to this course offering.");

            _unitOfWork.AssessmentModules.Delete(module);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
