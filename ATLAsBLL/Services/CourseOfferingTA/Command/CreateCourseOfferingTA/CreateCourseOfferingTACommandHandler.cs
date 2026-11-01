using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.CreateCourseOfferingTA
{
    /// <summary>
    /// Create Course Offering TA Command Handler.
    /// </summary>
    public class CreateCourseOfferingTACommandHandler : IRequestHandler<CreateCourseOfferingTACommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateCourseOfferingTACommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handles assigning a teaching assistant to a course offering with institution and role validation.
        /// </summary>
        public async Task<int> Handle(CreateCourseOfferingTACommand request, CancellationToken cancellationToken)
        {
            var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(request.CourseOfferingId, cancellationToken);
            if (offering == null)
            {
                throw new KeyNotFoundException($"Course offering with ID {request.CourseOfferingId} was not found.");
            }

            // Tenant isolation: Non-SAdmin callers can only manage offerings within their own institution
            if (!_currentUserService.IsSAdmin)
            {
                if (!offering.InstitutionID.HasValue)
                {
                    throw new ForbiddenAccessException("Course offering does not have an assigned institution.");
                }

                if (offering.InstitutionID.Value != _currentUserService.InstitutionId)
                {
                    throw new ForbiddenAccessException("You are not authorized to manage course offerings outside your institution.");
                }
            }

            var ta = await _unitOfWork.Users.GetByIdAsync(request.TAID, cancellationToken);
            if (ta == null)
            {
                throw new KeyNotFoundException($"User with ID {request.TAID} was not found.");
            }

            if (!ta.IsActive)
            {
                throw new ArgumentException("The target user account is inactive.");
            }

            if (ta.Role != enUserRole.TeachingAssistant)
            {
                throw new ArgumentException($"User with ID {request.TAID} does not have the TeachingAssistant role.");
            }

            // Assignee institution check
            if (!_currentUserService.IsSAdmin && ta.InstitutionID != _currentUserService.InstitutionId)
            {
                throw new ForbiddenAccessException("Cannot assign a teaching assistant belonging to a different institution.");
            }

            if (offering.InstitutionID.HasValue && ta.InstitutionID != offering.InstitutionID.Value)
            {
                throw new ArgumentException("Teaching assistant does not belong to the course offering's institution.");
            }

            // Prevent duplicate assignment
            var existingAssignments = await _unitOfWork.CourseOfferingTAs.FindAsync(
                cot => cot.CourseOfferingID == request.CourseOfferingId && cot.TeacherAssistID == request.TAID,
                cancellationToken);

            if (existingAssignments.Any())
            {
                throw new InvalidOperationException("Teaching assistant is already assigned to this course offering.");
            }

            var cot = new CourseOfferingTA
            {
                CourseOfferingID = request.CourseOfferingId,
                TeacherAssistID = request.TAID
            };

            await _unitOfWork.CourseOfferingTAs.AddAsync(cot, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return cot.CourseOfferingTAID;
        }
    }
}
