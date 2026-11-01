using ATLASDomain.DTO.Semester;
using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Features.Semesters.Queries.GetSemesters
{
    /// <summary>
    /// Get Semesters Query Handler.
    /// Scopes returned semesters strictly to the caller's institution for non-SAdmin users.
    /// </summary>
    public class GetSemestersQueryHandler : IRequestHandler<GetSemestersQuery, List<SemesterDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetSemestersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<List<SemesterDto>> Handle(GetSemestersQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsSAdmin)
            {
                var userInstId = _currentUserService.InstitutionId;
                if (!userInstId.HasValue || userInstId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User is not associated with any institution.");
                }

                var semesters = await _unitOfWork.Semesters.FindAsync(
                    s => s.InstitutionID == userInstId.Value,
                    cancellationToken);

                return semesters.Select(s => new SemesterDto
                {
                    SemesterID = s.SemesterID,
                    SemesterName = s.SemesterName,
                    AcademicYear = s.AcademicYear,
                    IsGradingClosed = s.IsGradingClosed,
                    InstitutionID = s.InstitutionID
                }).ToList();
            }
            else
            {
                var semesters = await _unitOfWork.Semesters.GetAllAsync(cancellationToken);
                return semesters.Select(s => new SemesterDto
                {
                    SemesterID = s.SemesterID,
                    SemesterName = s.SemesterName,
                    AcademicYear = s.AcademicYear,
                    IsGradingClosed = s.IsGradingClosed,
                    InstitutionID = s.InstitutionID
                }).ToList();
            }
        }
    }
}

