using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using MediatR;

namespace ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest
{

    public class UpdateGradeAppealRequestCommandHandler
        : IRequestHandler<UpdateGradeAppealRequestCommand, bool>
    {
        private readonly IGradeAppealRequestRepository _repository;

        public UpdateGradeAppealRequestCommandHandler(
            IGradeAppealRequestRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(
            UpdateGradeAppealRequestCommand command,
            CancellationToken cancellationToken)
        {
            return await _repository.UpdateAsync(
                command.RequestId,
                command.DoctorUserId,
                command.Request);
        }
    }
}
