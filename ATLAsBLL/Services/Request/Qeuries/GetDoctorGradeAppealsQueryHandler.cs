using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Request;
using MediatR;

namespace ATLAsBLL.Services.Request.Qeuries
{

    /// <summary>
    /// Get Doctor Grade Appeals Query Handler.
    /// </summary>
    public class GetDoctorGradeAppealsQueryHandler
        : IRequestHandler<GetDoctorGradeAppealsQuery, List<GradeAppealRequestDto>>
    {
        /// <summary>
        /// _Unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _UnitOfWork;

        public GetDoctorGradeAppealsQueryHandler(
            IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<List<GradeAppealRequestDto>> Handle(
            GetDoctorGradeAppealsQuery query,
            CancellationToken cancellationToken)
        {
            return await _UnitOfWork.GradeAppealRequests.GetByDoctorIdAsync(query.DoctorUserId);
        }
    }
}
