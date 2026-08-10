using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange;
using Microsoft.EntityFrameworkCore;

namespace ATLAsBLL.Services.Enrollment.Command.CreateEnrollmentRange
{
    public class CreateEnrollmentsRangeCommandHandler : IRequestHandler<CreateEnrollmentsRangeCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateEnrollmentsRangeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CreateEnrollmentsRangeCommand request, CancellationToken cancellationToken)
        {
            var requestedPairs = request.Enrollments
                .Select(x => (x.StudentId, x.CourseOfferingId))
                .ToList();

            if (requestedPairs.Distinct().Count() != requestedPairs.Count)
                throw new InvalidOperationException("An enrollment batch cannot contain duplicate student and course offering pairs.");

            var studentIds = requestedPairs.Select(x => x.StudentId).Distinct().ToList();
            var courseOfferingIds = requestedPairs.Select(x => x.CourseOfferingId).Distinct().ToList();

            var students = await _unitOfWork.Students.FindAsync(x => studentIds.Contains(x.StudentId));
            if (students.Select(x => x.StudentId).Distinct().Count() != studentIds.Count)
                throw new KeyNotFoundException("One or more students do not exist.");

            var courseOfferings = await _unitOfWork.CourseOfferings.FindAsync(x => courseOfferingIds.Contains(x.CourseOfferingID));
            if (courseOfferings.Select(x => x.CourseOfferingID).Distinct().Count() != courseOfferingIds.Count)
                throw new KeyNotFoundException("One or more course offerings do not exist.");

            var existingEnrollments = await _unitOfWork.Enrollments.FindAsync(x =>
                studentIds.Contains(x.StudentID) && courseOfferingIds.Contains(x.CourseOfferingID));
            var existingPairs = existingEnrollments
                .Select(x => (x.StudentID, x.CourseOfferingID))
                .ToHashSet();

            if (requestedPairs.Any(existingPairs.Contains))
                throw new InvalidOperationException("One or more students are already enrolled in the requested course offering.");

            var enrollmentsList = new List<ATLASDomain.Entities.Enrollment>();

            foreach (var item in request.Enrollments)
            {
                var enrollment = new ATLASDomain.Entities.Enrollment
                {
                    StudentID = item.StudentId,
                    CourseOfferingID = item.CourseOfferingId,
                    EnrollmentDate = DateTime.UtcNow
                };
                enrollmentsList.Add(enrollment);
            }

            try
            {
                await _unitOfWork.Enrollments.AddRangeAsync(enrollmentsList);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException ex) when (IsEnrollmentUniquenessViolation(ex))
            {
                // The unique index is the final guard when another transaction enrolled a student after our read.
                throw new InvalidOperationException("One or more students are already enrolled in the requested course offering.", ex);
            }
        }

        private static bool IsEnrollmentUniquenessViolation(DbUpdateException exception) =>
            exception.InnerException?.Message.Contains("UX_Enrollments_StudentID_CourseOfferingID", StringComparison.OrdinalIgnoreCase) == true;
    }
}
