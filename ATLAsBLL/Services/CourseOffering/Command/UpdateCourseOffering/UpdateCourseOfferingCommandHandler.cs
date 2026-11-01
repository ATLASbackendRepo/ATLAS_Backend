using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Features.CourseOfferings.Commands.UpdateCourseOffering
{
    /// <summary>
    /// Update Course Offering Command Handler.
    /// </summary>
    public class UpdateCourseOfferingCommandHandler : IRequestHandler<UpdateCourseOfferingCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCourseOfferingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(UpdateCourseOfferingCommand request, CancellationToken cancellationToken)
        {
            var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(request.CourseOfferingId, cancellationToken);

            if (offering == null)
            {
                throw new KeyNotFoundException($"Course Offering with ID {request.CourseOfferingId} was not found.");
            }

            int targetInstitutionId;
            if (_currentUserService.IsSAdmin)
            {
                if (request.InstitutionId.HasValue && request.InstitutionId.Value > 0)
                {
                    targetInstitutionId = request.InstitutionId.Value;
                }
                else if (offering.InstitutionID.HasValue && offering.InstitutionID.Value > 0)
                {
                    targetInstitutionId = offering.InstitutionID.Value;
                }
                else
                {
                    throw new ArgumentException("Institution ID must be specified when updating an unassigned course offering as SAdmin.", nameof(request));
                }

                offering.InstitutionID = targetInstitutionId;
            }
            else
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }

                if (offering.InstitutionID == null || offering.InstitutionID.Value != _currentUserService.InstitutionId.Value)
                {
                    throw new ForbiddenAccessException("User is not authorized to update this course offering.");
                }

                targetInstitutionId = _currentUserService.InstitutionId.Value;
            }

            // 1. Verify Course exists and belongs to target institution
            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken);
            if (course == null)
            {
                throw new KeyNotFoundException($"Course with ID {request.CourseId} was not found.");
            }
            if (course.InstitutionID == null || course.InstitutionID.Value != targetInstitutionId)
            {
                throw new ArgumentException($"Course with ID {request.CourseId} does not belong to the target institution.");
            }

            // 2. Verify Semester exists and belongs to target institution
            var semester = await _unitOfWork.Semesters.GetByIdAsync(request.SemesterId, cancellationToken);
            if (semester == null)
            {
                throw new KeyNotFoundException($"Semester with ID {request.SemesterId} was not found.");
            }
            if (semester.InstitutionID == null || semester.InstitutionID.Value != targetInstitutionId)
            {
                throw new ArgumentException($"Semester with ID {request.SemesterId} does not belong to the target institution.");
            }

            // 3. Verify cross-institution consistency
            if (course.InstitutionID != semester.InstitutionID)
            {
                throw new ArgumentException("Course and Semester must belong to the same institution.");
            }

            // 4. Check duplicate offering
            var existingOfferings = await _unitOfWork.CourseOfferings.GetBySemesterAsync(request.SemesterId, cancellationToken);
            if (existingOfferings != null && existingOfferings.Any(o => o.CourseOfferingID != offering.CourseOfferingID && o.CourseID == request.CourseId && o.InstitutionID == targetInstitutionId))
            {
                throw new InvalidOperationException($"A course offering for course ID {request.CourseId} in semester ID {request.SemesterId} already exists.");
            }

            offering.CourseID = request.CourseId;
            offering.SemesterID = request.SemesterId;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
