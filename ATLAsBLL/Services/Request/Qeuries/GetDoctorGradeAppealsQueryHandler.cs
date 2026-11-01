using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Request;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Request.Qeuries
{
    /// <summary>
    /// Get Doctor Grade Appeals Query Handler.
    /// </summary>
    public class GetDoctorGradeAppealsQueryHandler
        : IRequestHandler<GetDoctorGradeAppealsQuery, List<GradeAppealRequestDto>>
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetDoctorGradeAppealsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _UnitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<List<GradeAppealRequestDto>> Handle(
            GetDoctorGradeAppealsQuery query,
            CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsSAdmin)
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }

                if (_currentUserService.UserId.HasValue && query.DoctorUserId != _currentUserService.UserId.Value)
                {
                    throw new ForbiddenAccessException("Cannot query appeals for another user.");
                }
            }

            return await _UnitOfWork.GradeAppealRequests.GetByDoctorIdAsync(query.DoctorUserId, cancellationToken);
        }
    }
}
