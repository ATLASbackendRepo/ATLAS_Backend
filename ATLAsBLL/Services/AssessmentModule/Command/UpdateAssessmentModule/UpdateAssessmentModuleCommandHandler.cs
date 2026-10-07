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
            
            var module = await _unitOfWork.AssessmentModules.GetByIdAsync(request.DTO.ModuleId, cancellationToken);
            if (module == null) throw new System.Collections.Generic.KeyNotFoundException("Module not found.");

            if (_unitOfWork.Semesters != null && await _unitOfWork.Semesters.IsGradingClosedForOfferingAsync(module.CourseOfferingID, cancellationToken))
            {
                throw new System.InvalidOperationException("Grading is closed for the semester of this course offering.");
            }
            var doctors = await _unitOfWork.CourseOfferingDoctors.GetDoctorsByOfferingAsync(module.CourseOfferingID, cancellationToken);
            var teachingAssistants = await _unitOfWork.CourseOfferingTAs.GetTAsByOfferingAsync(module.CourseOfferingID, cancellationToken);
            if (!doctors.Contains(request.UserId) && !teachingAssistants.Any(ta => ta.TeacherAssistID == request.UserId))
                throw new UnauthorizedAccessException("You are not assigned to this course offering.");
            if (request.DTO.MaxDegree < AssessmentModule.MinimumMaxDegree || request.DTO.MaxDegree > AssessmentModule.MaximumMaxDegree)
            {
                throw new ArgumentException($"MaxDegree must be a value between {AssessmentModule.MinimumMaxDegree} and {AssessmentModule.MaximumMaxDegree}.");
            }
            if (string.IsNullOrWhiteSpace(request.DTO.ModuleName))
            {
                throw new ArgumentException("ModuleName cannot be null or empty.");
            }

            // Lowering MaxDegree below a grade that already exists would silently
            // invalidate the recorded grade, so it is refused (MED-07).
            var maxRecordedDegree = await _unitOfWork.AssessmentModules
                .GetMaxDegreeByModuleAsync(module.ModuleID, cancellationToken);

            if (request.DTO.MaxDegree < maxRecordedDegree)
            {
                throw new ArgumentException(
                    $"MaxDegree cannot be lowered to {request.DTO.MaxDegree} because a grade of {maxRecordedDegree} is already recorded for this module.");
            }

            // The modules of one offering compose a single course grade, so their
            // max degrees may add up to at most 100 (MED-07).
            var offeringTotal = await _unitOfWork.AssessmentModules
                .GetTotalMaxDegreeByCourseOfferingAsync(module.CourseOfferingID, cancellationToken);

            if (offeringTotal - module.MaxDegree + request.DTO.MaxDegree > AssessmentModule.MaximumOfferingTotalMaxDegree)
            {
                throw new ArgumentException(
                    $"The total max degree of all modules in a course offering cannot exceed {AssessmentModule.MaximumOfferingTotalMaxDegree}.");
            }

            module.ModuleName = request.DTO.ModuleName;
            module.MaxDegree = request.DTO.MaxDegree;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
