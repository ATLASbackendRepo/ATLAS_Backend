using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Request;
using MediatR;

namespace ATLAsBLL.Services.Request.Qeuries
{

    /// <summary>
    /// Get Grade Appeal By Id Query Handler.
    /// </summary>
    public class GetGradeAppealByIdQueryHandler
        : IRequestHandler<GetGradeAppealByIdQuery, GradeAppealRequestDto?>
    {
        /// <summary>
        /// _Unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _UnitOfWork;

        public GetGradeAppealByIdQueryHandler(
            IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<GradeAppealRequestDto?> Handle(
            GetGradeAppealByIdQuery query,
            CancellationToken cancellationToken)
        {
            var request = await _UnitOfWork.GradeAppealRequests.GetByIdAsync(query.RequestId, cancellationToken);

            if (request is null)
                return null;

            
            bool canView = request.DoctorUserID == query.CurrentUserId;

            return canView ? request : null;
        }
    }
}
