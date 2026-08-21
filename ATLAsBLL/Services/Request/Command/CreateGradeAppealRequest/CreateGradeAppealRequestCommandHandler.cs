using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Request.Command.CreateGradeAppealRequest
{
    public class CreateGradeAppealRequestCommandHandler
        : IRequestHandler<CreateGradeAppealRequestCommand, int>
    {
        private readonly IUnitOfWork _UnitOfWork;

        public CreateGradeAppealRequestCommandHandler(
            IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        public async Task<int> Handle(
            CreateGradeAppealRequestCommand command,
            CancellationToken cancellationToken)
        {
            // Add validation here:
            // - Student is enrolled in CourseOfferingID
            // - Selected doctor teaches CourseOfferingID
            // - No open appeal exists for this assessment
            
            var DoctorIds = await _UnitOfWork.CourseOfferingDoctors
                .GetDoctorIdsByCourseOfferingIdAsync(command.Request.CourseOfferingID);


            if (!DoctorIds.Contains(command.Request.DoctorUserID))
            {
                throw new ForbiddenAccessException("Selected doctor does not teach the specified course offering.");
            }

            if (command.Request.CourseOfferingID <= 0)
            {
                throw new ArgumentException("Invalid Request ");
            }

            ATLASDomain.Entities.Student student = await _UnitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == command.UserId);

            return await _UnitOfWork.GradeAppealRequests.AddAsync(
                command.Request,
                student.StudentId);
        }
    }
}
