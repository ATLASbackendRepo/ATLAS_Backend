using ATLASDomain.DTO.Request;
using MediatR;

namespace ATLAsBLL.Services.Request.Qeuries
{

    public class GetDoctorGradeAppealsQueryHandler
        : IRequestHandler<GetDoctorGradeAppealsQuery, List<GradeAppealRequestDto>>
    {
        private readonly IGradeAppealRequestRepository _repository;

        public GetDoctorGradeAppealsQueryHandler(
            IGradeAppealRequestRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<GradeAppealRequestDto>> Handle(
            GetDoctorGradeAppealsQuery query,
            CancellationToken cancellationToken)
        {
            return await _repository.GetByDoctorIdAsync(query.DoctorUserId);
        }
    }
}
