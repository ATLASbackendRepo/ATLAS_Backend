using MediatR;
using ATLAsDCL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Features.Courses.Commands.UpdateCourse
{
    /// <summary>
    /// Update Course Command Handler.
    /// </summary>
    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCourseCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            if (request.UpdateCourseDTO == null || string.IsNullOrWhiteSpace(request.UpdateCourseDTO.CourseName))
            {
                throw new ArgumentNullException(nameof(request.UpdateCourseDTO.CourseName));
            }

            var course = await _unitOfWork.Courses.GetByIdAsync(request.UpdateCourseDTO.Id, cancellationToken);

            if (course == null)
            {
                throw new KeyNotFoundException($"Course with ID {request.UpdateCourseDTO.Id} was not found.");
            }

            if (!_currentUserService.IsSAdmin)
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }

                if (course.InstitutionID == null || course.InstitutionID.Value != _currentUserService.InstitutionId.Value)
                {
                    throw new ForbiddenAccessException("User is not authorized to update this course.");
                }
            }

            var trimmedName = request.UpdateCourseDTO.CourseName.Trim();
            var targetInstitutionId = course.InstitutionID;

            if (targetInstitutionId.HasValue)
            {
                var duplicateExists = await _unitOfWork.Courses.AnyAsync(
                    c => c.CourseID != course.CourseID &&
                         c.InstitutionID == targetInstitutionId.Value &&
                         c.C_Name.ToLower() == trimmedName.ToLower(),
                    cancellationToken);

                if (duplicateExists)
                {
                    throw new InvalidOperationException($"A course with the name '{trimmedName}' already exists in this institution.");
                }
            }

            course.C_Name = trimmedName;
            course.Credit_Hours = request.UpdateCourseDTO.CreditHours;
            course.IsPractical = request.UpdateCourseDTO.IsPractical;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
