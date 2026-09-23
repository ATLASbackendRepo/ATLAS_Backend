using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;

namespace ATLAsBLL.Features.AssessmentModules.Commands.UpdateAssessmentModule
{
    /// <summary>
    /// Update Assessment Module Command Handler.
    /// </summary>
    public class UpdateAssessmentModuleCommandHandler : IRequestHandler<UpdateAssessmentModuleCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        public UpdateAssessmentModuleCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(UpdateAssessmentModuleCommand request, CancellationToken cancellationToken)
        {
            
            var module = await _unitOfWork.AssessmentModules.GetByIdAsync(request.DTO.ModuleId);
            if (module == null) throw new System.Collections.Generic.KeyNotFoundException("Module not found.");
            var doctors = await _unitOfWork.CourseOfferingDoctors.GetDoctorsByOfferingAsync(module.CourseOfferingID);
            var teachingAssistants = await _unitOfWork.CourseOfferingTAs.GetTAsByOfferingAsync(module.CourseOfferingID);
            if (!doctors.Contains(request.UserId) && !teachingAssistants.Any(ta => ta.TeacherAssistID == request.UserId))
                throw new UnauthorizedAccessException("You are not assigned to this course offering.");
            if (request.DTO.MaxDegree < ATLASDomain.Entities.AssessmentModule.MinimumMaxDegree || request.DTO.MaxDegree > ATLASDomain.Entities.AssessmentModule.MaximumMaxDegree)
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
