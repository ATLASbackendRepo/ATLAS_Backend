using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Features.Semesters.Commands.CreateSemester
{
    /// <summary>
    /// Create Semester Command Handler.
    /// Scopes created semester strictly to the caller's institution for non-SAdmin users,
    /// and requires SAdmin callers to explicitly specify the target institution.
    /// </summary>
    public class CreateSemesterCommandHandler : IRequestHandler<CreateSemesterCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateSemesterCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<int> Handle(CreateSemesterCommand request, CancellationToken cancellationToken)
        {
            int targetInstitutionId;
            if (_currentUserService.IsSAdmin)
            {
                if (!request.InstitutionId.HasValue || request.InstitutionId.Value <= 0)
                {
                    throw new ArgumentException("Target institution ID is required when creating a semester.");
                }
                targetInstitutionId = request.InstitutionId.Value;
            }
            else
            {
                var userInstId = _currentUserService.InstitutionId;
                if (!userInstId.HasValue || userInstId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User is not associated with any institution.");
                }
                targetInstitutionId = userInstId.Value;
            }

            var duplicateExists = await _unitOfWork.Semesters.AnyAsync(
                s => s.SemesterName == request.SemesterName &&
                     s.AcademicYear == request.AcademicYear &&
                     s.InstitutionID == targetInstitutionId,
                cancellationToken);

            if (duplicateExists)
            {
                throw new InvalidOperationException(
                    $"Semester '{request.SemesterName}' of academic year '{request.AcademicYear}' already exists in this institution.");
            }

            var semester = new Semester
            {
                SemesterName = request.SemesterName,
                AcademicYear = request.AcademicYear,
                InstitutionID = targetInstitutionId,
                IsGradingClosed = false
            };

            await _unitOfWork.Semesters.AddAsync(semester, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return semester.SemesterID;
        }
    }
}
