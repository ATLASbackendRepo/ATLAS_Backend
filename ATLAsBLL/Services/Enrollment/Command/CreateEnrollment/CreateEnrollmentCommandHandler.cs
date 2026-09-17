using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment
{
    public class CreateEnrollmentCommandHandler : IRequestHandler<CreateEnrollmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateEnrollmentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
        {

            if (request.BulkCreateEnrollmentDTO1 == null)
                throw new Exception();

            var ISInserted = await _unitOfWork.Enrollments.AddSingleStudentToEnrollmentAsync(request.AcdimicID, request.BulkCreateEnrollmentDTO1);



            return ISInserted;
        }
    }
}