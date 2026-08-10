using MediatR;
namespace ATLAsBLL.Services.Request.Qeuries
{

    public class GetGradeAppealByIdQueryHandler
        : IRequestHandler<GetGradeAppealByIdQuery, GradeAppealRequestDto?>
    {
        private readonly IGradeAppealRequestRepository _repository;

        public GetGradeAppealByIdQueryHandler(
            IGradeAppealRequestRepository repository)
        {
            _repository = repository;
        }

        public async Task<GradeAppealRequestDto?> Handle(
            GetGradeAppealByIdQuery query,
            CancellationToken cancellationToken)
        {
            var request = await _repository.GetByIdAsync(query.RequestId);

            if (request is null)
                return null;

            // Student or assigned doctor can view the request.
            bool canView = query.IsSuperAdmin ||
                           request.StudentID == query.CurrentStudentId ||
                           request.DoctorUserID == query.CurrentUserId;

            return canView ? request : null;
        }
    }
}
