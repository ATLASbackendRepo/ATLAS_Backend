using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Features.CourseGrades.Commands.UpdateCourseGrade
{
    public class AssignCourseGradeCommandHandler : IRequestHandler<AssignCourseGradeCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignCourseGradeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(AssignCourseGradeCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(request.EnrollmentId);
            if (enrollment is null)
                throw new KeyNotFoundException($"Enrollment with ID {request.EnrollmentId} does not exist.");

            var module = await _unitOfWork.AssessmentModules.GetByIdAsync(request.ModuleId);
            if (module is null)
                throw new KeyNotFoundException($"Assessment module with ID {request.ModuleId} does not exist.");

            if (enrollment.CourseOfferingID != module.CourseOfferingID)
                throw new InvalidOperationException("The assessment module does not belong to the enrollment's course offering.");

            if (request.StudentDegree < 0 || request.StudentDegree > module.MaxDegree)
                throw new InvalidOperationException("Student degree cannot exceed the assessment module's maximum degree.");

            var grade = await _unitOfWork.CourseGrades.GetGradeByEnrollmentAndModuleAsync(request.EnrollmentId, request.ModuleId);

            if (grade == null)
            {
                var courseGrade = new CourseGrade
                {
                    EnrollmentID = request.EnrollmentId,
                    ModuleID = request.ModuleId,
                    StudentDegree = request.StudentDegree,
                    CreatedBy = request.UserId
                };

                await _unitOfWork.CourseGrades.AddAsync(courseGrade);
            }
            else
            {
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
                throw new InvalidOperationException("This grade was changed by another user. Refresh and try again.", ex);
            }
        }
    }
}
