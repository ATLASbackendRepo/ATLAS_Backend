using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment
{
    /// <summary>
    /// Create Enrollment Command Handler.
    /// </summary>
    public class CreateEnrollmentCommandHandler : IRequestHandler<CreateEnrollmentCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public CreateEnrollmentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
        {

            if (request.BulkCreateEnrollmentDTO1 == null)
                throw new Exception();

            var ISInserted = await _unitOfWork.Enrollments.AddSingleStudentToEnrollmentAsync(request.AcdimicID, request.BulkCreateEnrollmentDTO1);



            return ISInserted;
        }
    }
}
