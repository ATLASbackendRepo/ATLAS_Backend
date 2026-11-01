using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Features.Semesters.Commands.DeleteSemester
{
    /// <summary>
    /// Delete Semester Command Handler.
    /// Scopes deletion by institution ownership and checks for dependent offerings.
    /// </summary>
    public class DeleteSemesterCommandHandler : IRequestHandler<DeleteSemesterCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteSemesterCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<bool> Handle(DeleteSemesterCommand request, CancellationToken cancellationToken)
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
                throw new KeyNotFoundException($"Semester with ID {request.SemesterID} does not exist.");
            }

            if (!_currentUserService.IsSAdmin)
            {
                if (!semester.InstitutionID.HasValue)
                {
                    throw new ForbiddenAccessException("You are not authorized to delete unassigned legacy semesters.");
                }

                if (semester.InstitutionID.Value != _currentUserService.InstitutionId.Value)
                {
                    throw new ForbiddenAccessException("You are not authorized to delete semesters outside your institution.");
                }
            }

            var hasOfferings = await _unitOfWork.CourseOfferings.AnyAsync(
                co => co.SemesterID == request.SemesterID,
                cancellationToken);

            if (hasOfferings)
            {
                throw new InvalidOperationException(
                    $"Cannot delete semester with ID {request.SemesterID} because course offerings are associated with it.");
            }

            _unitOfWork.Semesters.Delete(semester);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
