using ATLASDomain.DTO.Semester;
using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Features.Semesters.Queries.GetSemesterById
{
    /// <summary>
    /// Get Semester By Id Query Handler.
    /// Scopes retrieval strictly by caller's institution for non-SAdmin users.
    /// </summary>
    public class GetSemesterByIdQueryHandler : IRequestHandler<GetSemesterByIdQuery, SemesterDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetSemesterByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<SemesterDto> Handle(GetSemesterByIdQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsSAdmin)
            {
                var userInstId = _currentUserService.InstitutionId;
                if (!userInstId.HasValue || userInstId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User is not associated with any institution.");
                }
            }

            var semester = await _unitOfWork.Semesters.GetByIdAsync(request.Id, cancellationToken);
            if (semester == null)
            {
                throw new KeyNotFoundException($"Semester with ID {request.Id} was not found.");
            }

            if (!_currentUserService.IsSAdmin)
            {
                if (!semester.InstitutionID.HasValue)
                {
                    throw new ForbiddenAccessException("You are not authorized to view unassigned legacy semesters.");
                }

                if (semester.InstitutionID.Value != _currentUserService.InstitutionId.Value)
                {
                    throw new ForbiddenAccessException("You are not authorized to view semesters outside your institution.");
                }
            }

            return new SemesterDto
            {
                SemesterID = semester.SemesterID,
                SemesterName = semester.SemesterName,
                AcademicYear = semester.AcademicYear,
                IsGradingClosed = semester.IsGradingClosed,
                InstitutionID = semester.InstitutionID
            };
        }
    }
}

