using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.CreateCourseOfferingDoctor
{
    /// <summary>
    /// Create Course Offering Doctor Command Handler.
    /// </summary>
    public class CreateCourseOfferingDoctorCommandHandler : IRequestHandler<CreateCourseOfferingDoctorCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateCourseOfferingDoctorCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handles assigning a doctor to a course offering with institution and role validation.
        /// </summary>
        public async Task<int> Handle(CreateCourseOfferingDoctorCommand request, CancellationToken cancellationToken)
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

            var doctor = await _unitOfWork.Users.GetByIdAsync(request.DoctorId, cancellationToken);
            if (doctor == null)
            {
                throw new KeyNotFoundException($"User with ID {request.DoctorId} was not found.");
            }

            if (!doctor.IsActive)
            {
                throw new ArgumentException("The target user account is inactive.");
            }

            if (doctor.Role != enUserRole.Doctor)
            {
                throw new ArgumentException($"User with ID {request.DoctorId} does not have the Doctor role.");
            }

            // Assignee institution check
            if (!_currentUserService.IsSAdmin && doctor.InstitutionID != _currentUserService.InstitutionId)
            {
                throw new ForbiddenAccessException("Cannot assign a doctor belonging to a different institution.");
            }

            if (offering.InstitutionID.HasValue && doctor.InstitutionID != offering.InstitutionID.Value)
            {
                throw new ArgumentException("Doctor does not belong to the course offering's institution.");
            }

            // Prevent duplicate assignment
            var existingAssignments = await _unitOfWork.CourseOfferingDoctors.FindAsync(
                cod => cod.CourseOfferingID == request.CourseOfferingId && cod.DoctorID == request.DoctorId,
                cancellationToken);

            if (existingAssignments.Any())
            {
                throw new InvalidOperationException("Doctor is already assigned to this course offering.");
            }

            var cod = new CourseOfferingDoctor
            {
                CourseOfferingID = request.CourseOfferingId,
                DoctorID = request.DoctorId
            };

            await _unitOfWork.CourseOfferingDoctors.AddAsync(cod, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return cod.CourseOfferingDoctorID;
        }
    }
}
