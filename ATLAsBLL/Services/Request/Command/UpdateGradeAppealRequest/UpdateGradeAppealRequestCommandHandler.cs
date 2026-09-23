using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;
using MediatR;

namespace ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest
{

    /// <summary>
    /// Update Grade Appeal Request Command Handler.
    /// </summary>
    public class UpdateGradeAppealRequestCommandHandler
        : IRequestHandler<UpdateGradeAppealRequestCommand, bool>
    {
        /// <summary>
        /// _unitofwork.
        /// </summary>
        private readonly IUnitOfWork _unitofwork;

        public UpdateGradeAppealRequestCommandHandler(
            IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
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
