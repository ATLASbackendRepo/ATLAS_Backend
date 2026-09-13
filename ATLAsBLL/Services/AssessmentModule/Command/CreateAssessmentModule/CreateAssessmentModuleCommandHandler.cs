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
            var DoctorsID = await _unitOfWork.CourseOfferingDoctors.GetDoctorsByOfferingAsync(request.DTO.CourseOfferingId);
            if (!DoctorsID.ToList().Contains(request.UserId))
            {
                var teachingAssistants = await _unitOfWork.CourseOfferingTAs.GetTAsByOfferingAsync(request.DTO.CourseOfferingId);
                if (!teachingAssistants.Any(ta => ta.TeacherAssistID == request.UserId))
                    throw new UnauthorizedAccessException("You are not assigned to the specified course offering.");
            }
            if (request.DTO.MaxDegree < AssessmentModule.MinimumMaxDegree || request.DTO.MaxDegree > AssessmentModule.MaximumMaxDegree)
            {
                throw new ArgumentException("MaxDegree must be a value between 0 and 100.");
            }
            if(string.IsNullOrWhiteSpace(request.DTO.ModuleName))
            {
                throw new ArgumentException("ModuleName cannot be null or empty.");
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
