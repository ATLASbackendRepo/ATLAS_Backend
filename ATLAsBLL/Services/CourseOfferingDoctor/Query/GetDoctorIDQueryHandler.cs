using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseOfferingDoctor.Query
{
    /// <summary>
    /// Get Doctor IDQuery Handler.
    /// </summary>
    public class GetDoctorIDQueryHandler : IRequestHandler<GetDoctorIDQuery, IEnumerable<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetDoctorIDQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<IEnumerable<int>> Handle(GetDoctorIDQuery request, CancellationToken cancellationToken)
        {
            var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(request.CourseOfferingID, cancellationToken);
            if (offering == null)
            {
                throw new KeyNotFoundException($"Course offering with ID {request.CourseOfferingID} was not found.");
            }

            // Tenant isolation: caller must belong to the offering's institution (unless SAdmin)
            if (!_currentUserService.IsSAdmin)
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }

                if (!offering.InstitutionID.HasValue || offering.InstitutionID.Value != _currentUserService.InstitutionId.Value)
                {
                    throw new ForbiddenAccessException("Course offering does not belong to your institution.");
                }
            }

            // If caller is in Student role (or student user), verify active enrollment in that course offering
            if (!_currentUserService.IsSAdmin && _currentUserService.Role == enUserRole.Student)
            {
                var userId = _currentUserService.UserId ?? 0;
                var student = await _unitOfWork.Students.GetStudentByUserIDAsync(userId, cancellationToken);
                if (student == null)
                {
                    throw new ForbiddenAccessException("Student record not found.");
                }

                if (offering.InstitutionID.HasValue && student.InstitutionID != offering.InstitutionID.Value)
                {
                    throw new ForbiddenAccessException("Student does not belong to the course offering's institution.");
                }

                var studentWithEnrollments = await _unitOfWork.Students.GetStudentWithEnrollmentsAsync(student.StudentId, cancellationToken);
                var isEnrolled = studentWithEnrollments?.Enrollments?.Any(e => e.CourseOfferingID == request.CourseOfferingID) == true;
                if (!isEnrolled)
                {
                    throw new ForbiddenAccessException("You are not enrolled in the specified course offering.");
                }
            }

            var doctorIDs = await _unitOfWork.CourseOfferingDoctors.GetDoctorsByOfferingAsync(request.CourseOfferingID, cancellationToken);
            if (doctorIDs == null || !doctorIDs.Any())
                throw new ArgumentException("No doctors found for the specified course offering.");

            return doctorIDs;
        }
    }
}
