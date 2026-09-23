using ATLAsDCL.UnitOfWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseOfferingDoctor.Query
{
    /// <summary>
    /// Get Doctor IDQuery Handler.
    /// </summary>
    internal class GetDoctorIDQueryHandler : IRequestHandler<GetDoctorIDQuery, IEnumerable<int>>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public GetDoctorIDQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<IEnumerable<int>> Handle(GetDoctorIDQuery request, CancellationToken cancellationToken)
        {
            var DoctorIDs = await _unitOfWork.CourseOfferingDoctors.GetDoctorsByOfferingAsync(request.CourseOfferingID);
            if (DoctorIDs == null || !DoctorIDs.Any())
                throw new ArgumentException("No doctors found for the specified course offering.");

            return DoctorIDs;
        }
    }
}
