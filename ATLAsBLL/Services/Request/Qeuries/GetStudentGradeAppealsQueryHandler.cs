
using MediatR;
namespace ATLAsBLL.Services.Request.Qeuries
{

    public class GetStudentGradeAppealsQueryHandler
        : IRequestHandler<GetStudentGradeAppealsQuery, List<GradeAppealRequestDto>>
    {
        private readonly IGradeAppealRequestRepository _repository;

        public GetStudentGradeAppealsQueryHandler(
            IGradeAppealRequestRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<GradeAppealRequestDto>> Handle(
            GetStudentGradeAppealsQuery query,
            CancellationToken cancellationToken)
        {
            return await _repository.GetByStudentIdAsync(query.StudentId);
        }
    }
}
