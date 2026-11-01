using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Request.Command.CreateGradeAppealRequest
{
    /// <summary>
    /// Create Grade Appeal Request Command Handler.
    /// </summary>
    public class CreateGradeAppealRequestCommandHandler
        : IRequestHandler<CreateGradeAppealRequestCommand, int>
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateGradeAppealRequestCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _UnitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<int> Handle(
            CreateGradeAppealRequestCommand command,
            CancellationToken cancellationToken)
        {
            if (command.Request.CourseOfferingID <= 0 || !command.Request.AssessmentModuleID.HasValue)
                throw new ArgumentException("A course offering and assessment module are required.");

            if (!_currentUserService.IsSAdmin)
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }

                if (_currentUserService.UserId.HasValue && command.UserId != _currentUserService.UserId.Value)
                {
                    throw new ForbiddenAccessException("Cannot submit appeal for another user.");
                }
            }

            var offering = await _UnitOfWork.CourseOfferings.GetByIdAsync(command.Request.CourseOfferingID, cancellationToken);
            if (offering == null)
            {
                throw new KeyNotFoundException($"Course offering with ID {command.Request.CourseOfferingID} was not found.");
            }

            if (!_currentUserService.IsSAdmin && offering.InstitutionID.HasValue && offering.InstitutionID.Value != _currentUserService.InstitutionId!.Value)
            {
                throw new ForbiddenAccessException("Course offering does not belong to your institution.");
            }

            var DoctorIds = await _UnitOfWork.CourseOfferingDoctors
                .GetDoctorIdsByCourseOfferingIdAsync(command.Request.CourseOfferingID, cancellationToken);

            if (!DoctorIds.Contains(command.Request.DoctorUserID))
            {
                throw new ForbiddenAccessException("Selected doctor does not teach the specified course offering.");
            }

            var studentIdentity = await _UnitOfWork.Students.GetStudentByUserIDAsync(command.UserId, cancellationToken);

            if (studentIdentity is null)
            {
                throw new KeyNotFoundException($"Student with ID '{command.UserId}' was not found.");
            }

            if (!_currentUserService.IsSAdmin && studentIdentity.InstitutionID != _currentUserService.InstitutionId!.Value)
            {
                throw new ForbiddenAccessException("Student does not belong to your institution.");
            }

            var student = await _UnitOfWork.Students.GetStudentWithEnrollmentsAsync(studentIdentity.StudentId, cancellationToken)
                ?? throw new KeyNotFoundException($"Student with ID '{command.UserId}' was not found.");

            var enrollment = student.Enrollments?
                .SingleOrDefault(x => x.CourseOfferingID == command.Request.CourseOfferingID);
            if (enrollment is null)
                throw new ForbiddenAccessException("You are not enrolled in the specified course offering.");

            var module = await _UnitOfWork.AssessmentModules.GetByIdAsync(command.Request.AssessmentModuleID.Value, cancellationToken);
            if (module is null || module.CourseOfferingID != command.Request.CourseOfferingID)
                throw new ArgumentException("The assessment module does not belong to the specified course offering.");

            if (await _UnitOfWork.GradeAppealRequests.HasOpenRequestAsync(student.StudentId, module.ModuleID, cancellationToken))
                throw new InvalidOperationException("An open appeal already exists for this assessment module.");

            var grade = await _UnitOfWork.CourseGrades.GetGradeByEnrollmentAndModuleAsync(enrollment.EnrollmentID, module.ModuleID, cancellationToken);
            if (grade is null)
                throw new KeyNotFoundException("No grade exists for the selected assessment module.");

            command.Request.CurrentGrade = grade.StudentDegree;

            return await _UnitOfWork.GradeAppealRequests.AddAsync(
                command.Request,
                student.StudentId, cancellationToken);
        }
    }
}
