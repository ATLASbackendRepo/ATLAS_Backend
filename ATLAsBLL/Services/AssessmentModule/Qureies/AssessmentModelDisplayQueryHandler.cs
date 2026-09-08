using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.AssessmentModule;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.AssessmentModule.Qureies
{
    internal class AssessmentModelDisplayQueryHandler : IRequestHandler<AssessmentModelDisplayQuery, IEnumerable<AssessmentModelDisplayDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AssessmentModelDisplayQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public async Task<IEnumerable<AssessmentModelDisplayDTO>> Handle(AssessmentModelDisplayQuery request, CancellationToken cancellationToken)
        {
            if (request.courseOfferingId <= 0) throw new ArgumentException("Invalid course offering ID.");
            var modules = await _unitOfWork.AssessmentModules.GetModulesByCourseOfferingAsync(request.courseOfferingId);
            if (!modules.Any()) throw new KeyNotFoundException("Module not found.");
         

            return modules;
        }
    }
}
