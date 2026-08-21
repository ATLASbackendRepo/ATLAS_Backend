using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Request;
using MediatR;
namespace ATLAsBLL.Services.Request.Qeuries
{

    public class GetGradeAppealByIdQueryHandler
        : IRequestHandler<GetGradeAppealByIdQuery, GradeAppealRequestDto?>
    {
        private readonly IUnitOfWork _UnitOfWork;

        public GetGradeAppealByIdQueryHandler(
            IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        public async Task<GradeAppealRequestDto?> Handle(
            GetGradeAppealByIdQuery query,
            CancellationToken cancellationToken)
        {
            var request = await _UnitOfWork.GradeAppealRequests.GetByIdAsync(query.RequestId);

            if (request is null)
                return null;

            
            bool canView = request.DoctorUserID == query.CurrentUserId;

            return canView ? request : null;
        }
    }
}
