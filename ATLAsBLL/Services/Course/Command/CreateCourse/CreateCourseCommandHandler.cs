using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Features.Courses.Commands.CreateCourse
{
    /// <summary>
    /// Create Course Command Handler.
    /// </summary>
    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateCourseCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<int> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            if (request.CreateCourseDTO == null || string.IsNullOrWhiteSpace(request.CreateCourseDTO.CourseName))
            {
                throw new ArgumentNullException(nameof(request.CreateCourseDTO.CourseName));
            }

            int targetInstitutionId;
            if (_currentUserService.IsSAdmin)
            {
                if (!request.CreateCourseDTO.InstitutionId.HasValue || request.CreateCourseDTO.InstitutionId.Value <= 0)
                {
                    throw new ArgumentException("Institution ID must be specified when creating a course as SAdmin.", nameof(request));
                }
                targetInstitutionId = request.CreateCourseDTO.InstitutionId.Value;
            }
            else
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }
                targetInstitutionId = _currentUserService.InstitutionId.Value;
            }

            var trimmedName = request.CreateCourseDTO.CourseName.Trim();
            var duplicateExists = await _unitOfWork.Courses.AnyAsync(
                c => c.InstitutionID == targetInstitutionId && c.C_Name.ToLower() == trimmedName.ToLower(),
                cancellationToken);

            if (duplicateExists)
            {
                throw new InvalidOperationException($"A course with the name '{trimmedName}' already exists in this institution.");
            }

            var course = new Course
            {
                C_Name = trimmedName,
                Credit_Hours = request.CreateCourseDTO.CreditHours,
                IsPractical = request.CreateCourseDTO.IsPractical,
                InstitutionID = targetInstitutionId
            };

            await _unitOfWork.Courses.AddAsync(course, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return course.CourseID;
        }
    }
}
