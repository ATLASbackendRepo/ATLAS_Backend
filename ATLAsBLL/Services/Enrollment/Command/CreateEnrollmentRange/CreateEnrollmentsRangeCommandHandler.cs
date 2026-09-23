using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange;
using Microsoft.EntityFrameworkCore;

namespace ATLAsBLL.Services.Enrollment.Command.CreateEnrollmentRange
{
    /// <summary>
    /// Create Enrollments Range Command Handler.
    /// </summary>
    public class CreateEnrollmentsRangeCommandHandler : IRequestHandler<CreateEnrollmentsRangeCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public CreateEnrollmentsRangeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(CreateEnrollmentsRangeCommand request, CancellationToken cancellationToken)
        {
            if (request.BulkCreateEnrollment == null)
                return false;


            var RowAffected = await _unitOfWork.Enrollments.BulkEnrollStudentsFastAsync(request.BulkCreateEnrollment);

            if(RowAffected <1)
            {
              
            return false;
            }

            return true;
        }

    }
}
