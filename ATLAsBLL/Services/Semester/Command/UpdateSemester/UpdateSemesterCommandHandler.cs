using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Features.Semesters.Commands.UpdateSemester
{
    /// <summary>
    /// Update Semester Command Handler.
    /// Scopes updates strictly by institution ownership and enforces fail-closed checks.
    /// </summary>
    public class UpdateSemesterCommandHandler : IRequestHandler<UpdateSemesterCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateSemesterCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<bool> Handle(UpdateSemesterCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsSAdmin)
            {
                var userInstId = _currentUserService.InstitutionId;
                if (!userInstId.HasValue || userInstId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User is not associated with any institution.");
                }
            }

            var semester = await _unitOfWork.Semesters.GetByIdAsync(request.SemesterID, cancellationToken);

            if (semester == null)
            {
                throw new KeyNotFoundException($"Semester with ID {request.SemesterID} was not found.");
            }

            if (!_currentUserService.IsSAdmin)
            {
                if (!semester.InstitutionID.HasValue)
                {
                    throw new ForbiddenAccessException("You are not authorized to update unassigned legacy semesters.");
                }

                if (semester.InstitutionID.Value != _currentUserService.InstitutionId.Value)
                {
                    throw new ForbiddenAccessException("You are not authorized to update semesters outside your institution.");
                }
            }

            var duplicateExists = await _unitOfWork.Semesters.AnyAsync(
                s => s.SemesterID != request.SemesterID &&
                     s.SemesterName == request.SemesterName &&
                     s.AcademicYear == request.AcademicYear &&
                     s.InstitutionID == semester.InstitutionID,
                cancellationToken);

            if (duplicateExists)
            {
                throw new InvalidOperationException(
                    $"Another semester with name '{request.SemesterName}' and academic year '{request.AcademicYear}' already exists in this institution.");
            }

            semester.SemesterName = request.SemesterName;
            semester.AcademicYear = request.AcademicYear;
            semester.IsGradingClosed = request.IsGradingClosed;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
