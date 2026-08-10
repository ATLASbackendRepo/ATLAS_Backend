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
        private readonly IGradeAppealRequestRepository _repository;

        public CreateGradeAppealRequestCommandHandler(
            IGradeAppealRequestRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(
            CreateGradeAppealRequestCommand command,
            CancellationToken cancellationToken)
        {
            // Add validation here:
            // - Student is enrolled in CourseOfferingID
            // - Selected doctor teaches CourseOfferingID
            // - No open appeal exists for this assessment

            return await _repository.AddAsync(
                command.Request,
                command.StudentId);
        }
    }
}
