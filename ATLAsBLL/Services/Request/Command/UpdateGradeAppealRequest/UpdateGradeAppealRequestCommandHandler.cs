using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;
using MediatR;

namespace ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest
{

    public class UpdateGradeAppealRequestCommandHandler
        : IRequestHandler<UpdateGradeAppealRequestCommand, bool>
    {
        private readonly IUnitOfWork _unitofwork;

        public UpdateGradeAppealRequestCommandHandler(
            IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public async Task<bool> Handle(
            UpdateGradeAppealRequestCommand command,
            CancellationToken cancellationToken)
        {

           



            return await _unitofwork.GradeAppealRequests.UpdateAsync(
                command.RequestId,
                command.DoctorUserId,
                command.Request);
        }
    }
}
