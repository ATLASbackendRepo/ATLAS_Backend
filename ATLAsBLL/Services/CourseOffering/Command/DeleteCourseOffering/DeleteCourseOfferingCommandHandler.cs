using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Features.CourseOfferings.Commands.DeleteCourseOffering
{
    /// <summary>
    /// Delete Course Offering Command Handler.
    /// </summary>
    public class DeleteCourseOfferingCommandHandler : IRequestHandler<DeleteCourseOfferingCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCourseOfferingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(DeleteCourseOfferingCommand request, CancellationToken cancellationToken)
        {
            var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(request.CourseOfferingId, cancellationToken);

            if (offering == null)
            {
                throw new KeyNotFoundException($"Course Offering with ID {request.CourseOfferingId} does not exist.");
            }

            if (!_currentUserService.IsSAdmin)
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }

                if (offering.InstitutionID == null || offering.InstitutionID.Value != _currentUserService.InstitutionId.Value)
                {
                    throw new ForbiddenAccessException("User is not authorized to delete this course offering.");
                }
            }

            var hasEnrollments = await _unitOfWork.Enrollments.AnyAsync(e => e.CourseOfferingID == offering.CourseOfferingID, cancellationToken);
            if (hasEnrollments)
            {
                throw new InvalidOperationException("Cannot delete course offering because it has active enrollments.");
            }

            var hasDoctors = await _unitOfWork.CourseOfferingDoctors.AnyAsync(cod => cod.CourseOfferingID == offering.CourseOfferingID, cancellationToken);
            if (hasDoctors)
            {
                throw new InvalidOperationException("Cannot delete course offering because doctors are assigned to it.");
            }

            var hasTAs = await _unitOfWork.CourseOfferingTAs.AnyAsync(cota => cota.CourseOfferingID == offering.CourseOfferingID, cancellationToken);
            if (hasTAs)
            {
                throw new InvalidOperationException("Cannot delete course offering because teaching assistants are assigned to it.");
            }

            _unitOfWork.CourseOfferings.Delete(offering);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
