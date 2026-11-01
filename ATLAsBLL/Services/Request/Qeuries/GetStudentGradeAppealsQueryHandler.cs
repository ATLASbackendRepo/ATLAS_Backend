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
    /// Get Student Grade Appeals Query Handler.
    /// </summary>
    public class GetStudentGradeAppealsQueryHandler
        : IRequestHandler<GetStudentGradeAppealsQuery, List<GradeAppealRequestDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetStudentGradeAppealsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<List<GradeAppealRequestDto>> Handle(
            GetStudentGradeAppealsQuery query,
            CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsSAdmin)
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }

                if (_currentUserService.UserId.HasValue && query.UserId != _currentUserService.UserId.Value)
                {
                    throw new ForbiddenAccessException("Cannot query appeals for another user.");
                }
            }

            var student = await _unitOfWork.Students.GetStudentByUserIDAsync(query.UserId, cancellationToken);

            if (student is null)
            {
                throw new KeyNotFoundException(
                    $"Student with ID '{query.UserId}' was not found.");
            }

            if (!_currentUserService.IsSAdmin && student.InstitutionID != _currentUserService.InstitutionId!.Value)
            {
                throw new ForbiddenAccessException("Student does not belong to your institution.");
            }

            return await _unitOfWork.GradeAppealRequests.GetByStudentIdAsync(student.StudentId, cancellationToken);
        }
    }
}
