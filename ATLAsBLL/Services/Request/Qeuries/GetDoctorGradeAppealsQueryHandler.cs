using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Request;
using MediatR;

namespace ATLAsBLL.Services.Request.Qeuries
{

    public class GetDoctorGradeAppealsQueryHandler
        : IRequestHandler<GetDoctorGradeAppealsQuery, List<GradeAppealRequestDto>>
    {
        private readonly IUnitOfWork _UnitOfWork;

        public GetDoctorGradeAppealsQueryHandler(
            IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        public async Task<List<GradeAppealRequestDto>> Handle(
            GetDoctorGradeAppealsQuery query,
            CancellationToken cancellationToken)
        {
            return await _UnitOfWork.GradeAppealRequests.GetByDoctorIdAsync(query.DoctorUserId);
        }
    }
}
