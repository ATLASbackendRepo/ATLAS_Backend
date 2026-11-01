using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Request;
using ATLASDomain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Request.Qeuries
{
    /// <summary>
    /// Get Grade Appeal By Id Query Handler.
    /// </summary>
    public class GetGradeAppealByIdQueryHandler
        : IRequestHandler<GetGradeAppealByIdQuery, GradeAppealRequestDto?>
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetGradeAppealByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _UnitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<GradeAppealRequestDto?> Handle(
            GetGradeAppealByIdQuery query,
            CancellationToken cancellationToken)
        {
            var appeal = await _UnitOfWork.GradeAppealRequests.GetByIdAsync(query.RequestId, cancellationToken);

            if (appeal is null)
                return null;

            if (_currentUserService.IsSAdmin || query.IsSuperAdmin)
                return appeal;

            // Non-SAdmin callers must have a valid institution claim
            if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
            {
                throw new ForbiddenAccessException("User does not belong to a valid institution.");
            }

            // Verify institution isolation
            if (appeal.InstitutionID > 0 && appeal.InstitutionID != _currentUserService.InstitutionId.Value)
            {
                throw new ForbiddenAccessException("You do not have access to view this grade appeal.");
            }

            // Role-specific ownership checks
            var role = _currentUserService.Role;
            if (role == enUserRole.Student)
            {
                // Fix student appeal lookup: resolve student profile from caller user identity
                var student = await _UnitOfWork.Students.GetStudentByUserIDAsync(query.CurrentUserId, cancellationToken);
                if (student == null || appeal.StudentID != student.StudentId)
                {
                    throw new ForbiddenAccessException("You can only view your own grade appeals.");
                }
            }
            else if (role == enUserRole.Doctor)
            {
                if (appeal.DoctorUserID != query.CurrentUserId)
                {
                    throw new ForbiddenAccessException("You are not assigned to review this grade appeal.");
                }
            }

            return appeal;
        }
    }
}
