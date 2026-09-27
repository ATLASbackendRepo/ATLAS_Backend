using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.AssessmentModules.Commands.CreateAssessmentModule
{
    /// <summary>
    /// Create Assessment Module Command Handler.
    /// </summary>
    public class CreateAssessmentModuleCommandHandler : IRequestHandler<CreateAssessmentModuleCommand, int>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        public CreateAssessmentModuleCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<int> Handle(CreateAssessmentModuleCommand request, CancellationToken cancellationToken)
        {
            var DoctorsID = await _unitOfWork.CourseOfferingDoctors.GetDoctorsByOfferingAsync(request.DTO.CourseOfferingId);
            if (!DoctorsID.ToList().Contains(request.UserId))
            {
                var teachingAssistants = await _unitOfWork.CourseOfferingTAs.GetTAsByOfferingAsync(request.DTO.CourseOfferingId);
                if (!teachingAssistants.Any(ta => ta.TeacherAssistID == request.UserId))
                    throw new UnauthorizedAccessException("You are not assigned to the specified course offering.");
            }
            if (request.DTO.MaxDegree < AssessmentModule.MinimumMaxDegree || request.DTO.MaxDegree > AssessmentModule.MaximumMaxDegree)
            {
                throw new ArgumentException($"MaxDegree must be a value between {AssessmentModule.MinimumMaxDegree} and {AssessmentModule.MaximumMaxDegree}.");
            }
            if(string.IsNullOrWhiteSpace(request.DTO.ModuleName))
            {
                throw new ArgumentException("ModuleName cannot be null or empty.");
            }

            // The modules of one offering compose a single course grade, so their
            // max degrees may add up to at most 100 (MED-07).
            var offeringTotal = await _unitOfWork.AssessmentModules
                .GetTotalMaxDegreeByCourseOfferingAsync(request.DTO.CourseOfferingId);

            if (offeringTotal + request.DTO.MaxDegree > AssessmentModule.MaximumOfferingTotalMaxDegree)
            {
                throw new ArgumentException(
                    $"The total max degree of all modules in a course offering cannot exceed {AssessmentModule.MaximumOfferingTotalMaxDegree}.");
            }

            var module = new AssessmentModule
            {
                CourseOfferingID = request.DTO.CourseOfferingId,
                ModuleName = request.DTO.ModuleName ,
                MaxDegree = request.DTO.MaxDegree
            };
            await _unitOfWork.AssessmentModules.AddAsync(module);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return module.ModuleID;
        }
    }
}
