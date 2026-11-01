using ATLAsDCL.UnitOfWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATLAsBLL.Exceptions;

namespace ATLAsBLL.Features.Courses.Commands.DeleteCourse
{
    /// <summary>
    /// Delete Course Command Handler.
    /// </summary>
    public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCourseCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseID, cancellationToken);

            if (course == null)
            {
                throw new KeyNotFoundException($"Course with ID {request.CourseID} does not exist.");
            }

            if (!_currentUserService.IsSAdmin)
            {
                if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
                {
                    throw new ForbiddenAccessException("User does not belong to a valid institution.");
                }

                if (course.InstitutionID == null || course.InstitutionID.Value != _currentUserService.InstitutionId.Value)
                {
                    throw new ForbiddenAccessException("User is not authorized to delete this course.");
                }
            }

            var offerings = await _unitOfWork.CourseOfferings.GetByCourseAsync(course.CourseID, cancellationToken);
            if (offerings != null && offerings.Any())
            {
                throw new InvalidOperationException("Cannot delete course because it has associated course offerings.");
            }

            _unitOfWork.Courses.Delete(course);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
