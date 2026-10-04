using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Features.CourseGrades.Commands.UpdateCourseGrade
{
    /// <summary>
    /// Assign Course Grade Command Handler.
    /// </summary>
    public class AssignCourseGradeCommandHandler : IRequestHandler<AssignCourseGradeCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public AssignCourseGradeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(AssignCourseGradeCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(request.EnrollmentId, cancellationToken);
            if (enrollment is null)
                throw new KeyNotFoundException($"Enrollment with ID {request.EnrollmentId} does not exist.");

            var module = await _unitOfWork.AssessmentModules.GetByIdAsync(request.ModuleId, cancellationToken);
            if (module is null)
                throw new KeyNotFoundException($"Assessment module with ID {request.ModuleId} does not exist.");

            if (enrollment.CourseOfferingID != module.CourseOfferingID)
                throw new InvalidOperationException("The assessment module does not belong to the enrollment's course offering.");

            if (await _unitOfWork.Semesters.IsGradingClosedForOfferingAsync(module.CourseOfferingID, cancellationToken))
                throw new InvalidOperationException("Grading is closed for this semester; grades can no longer be assigned.");

            if (request.StudentDegree < 0 || request.StudentDegree > module.MaxDegree)
                throw new InvalidOperationException("Student degree cannot exceed the assessment module's maximum degree.");

            var grade = await _unitOfWork.CourseGrades.GetGradeByEnrollmentAndModuleAsync(request.EnrollmentId, request.ModuleId, cancellationToken);

            if (grade == null)
            {
                var courseGrade = new CourseGrade
                {
                    EnrollmentID = request.EnrollmentId,
                    ModuleID = request.ModuleId,
                    StudentDegree = request.StudentDegree,
                    CreatedBy = request.UserId
                };

                await _unitOfWork.CourseGrades.AddAsync(courseGrade, cancellationToken);

                // MED-06: the first value a grade ever takes is recorded too, with
                // no previous value, so the trail is complete rather than starting
                // from the second edit onwards.
                await _unitOfWork.CourseGradeHistory.AddAsync(new CourseGradeHistory
                {
                    CG_ID = courseGrade.CG_ID,
                    EnrollmentID = courseGrade.EnrollmentID,
                    ModuleID = courseGrade.ModuleID,
                    PreviousDegree = null,
                    NewDegree = request.StudentDegree,
                    ChangedBy = request.UserId
                }, cancellationToken);
            }
            else
            {
                // MED-06: the previous value is captured before it is overwritten.
                // Without this the audit log only ever recorded that "a grade
                // changed" and by whom, never what it had been, which is what made
                // a silent overwrite impossible to investigate after the fact.
                await _unitOfWork.CourseGradeHistory.AddAsync(new CourseGradeHistory
                {
                    CG_ID = grade.CG_ID,
                    EnrollmentID = grade.EnrollmentID,
                    ModuleID = grade.ModuleID,
                    PreviousDegree = grade.StudentDegree,
                    NewDegree = request.StudentDegree,
                    ChangedBy = request.UserId
                }, cancellationToken);

                grade.StudentDegree = request.StudentDegree;
                grade.UpdatedBy = request.UserId;
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // MED-06: the row version did not match, so somebody committed a
                // change while this one was in flight. The optimistic-concurrency
                // token turns last-write-wins into an explicit rejection.
                throw new InvalidOperationException(
                    "This grade was changed by another user. Refresh and try again.", ex);
            }
        }
    }
}
