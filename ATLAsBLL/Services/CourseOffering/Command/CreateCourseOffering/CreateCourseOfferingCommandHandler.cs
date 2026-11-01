using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Features.CourseOfferings.Commands.CreateCourseOffering
{
    /// <summary>
    /// Create Course Offering Command Handler.
    /// </summary>
    public class CreateCourseOfferingCommandHandler : IRequestHandler<CreateCourseOfferingCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateCourseOfferingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<int> Handle(CreateCourseOfferingCommand request, CancellationToken cancellationToken)
        {
            int targetInstitutionId;
            if (_currentUserService.IsSAdmin)
            {
                if (!request.InstitutionId.HasValue || request.InstitutionId.Value <= 0)
                {
                    throw new ArgumentException("Institution ID must be specified when creating a course offering as SAdmin.", nameof(request));
                }
                targetInstitutionId = request.InstitutionId.Value;
            }
            else
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
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

            // 3. Verify Course and Semester belong to the same institution
            if (course.InstitutionID != semester.InstitutionID)
            {
                throw new ArgumentException("Course and Semester must belong to the same institution.");
            }

            // 4. Check for duplicate offering (BE-17)
            var existingOfferings = await _unitOfWork.CourseOfferings.GetBySemesterAsync(request.SemesterId, cancellationToken);
            if (existingOfferings != null && existingOfferings.Any(o => o.CourseID == request.CourseId && o.InstitutionID == targetInstitutionId))
            {
                throw new InvalidOperationException($"A course offering for course ID {request.CourseId} in semester ID {request.SemesterId} already exists.");
            }

            var offering = new CourseOffering
            {
                CourseID = request.CourseId,
                SemesterID = request.SemesterId,
                InstitutionID = targetInstitutionId
            };

            await _unitOfWork.CourseOfferings.AddAsync(offering, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return offering.CourseOfferingID;
        }
    }
}
