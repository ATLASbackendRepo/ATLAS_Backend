using ATLAsBLL.Services.profile.Staff.command;
using ATLAsDCL.UnitOfWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.profile.Student.command
{
    /// <summary>
    /// update Student Profile Info Handler.
    /// </summary>
    public class updateStudentProfileInfoHandler : IRequestHandler<updateStudentProfileInfoCommand, bool>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public updateStudentProfileInfoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(
            updateStudentProfileInfoCommand request,
            CancellationToken cancellationToken)
        {
            var student = await _unitOfWork.Students.GetStudentByUserIDAsync(request.UserId, cancellationToken);

            if(student == null) 
                return false;

            var profile = await _unitOfWork.StudentProfile.GetByUserIdAsync(student.StudentId, cancellationToken);

            if (profile == null || profile.Student == null || profile.Student.User == null)
                return false;

            // The repository returns a tracked graph, so EF Core writes only the
            // properties changed here. Calling Update on the graph instead would
            // mark every column as modified and overwrite the user's password,
            // refresh-token hash and activation flag with stale values.
            profile.Student.User.FullName = request.StudentUpdateProfileInfoDTO.FullName;
            profile.Faculty = request.StudentUpdateProfileInfoDTO.Faculty;
            profile.SectionNumber = request.StudentUpdateProfileInfoDTO.SectionNumber;
            profile.LinkedInUrl = request.StudentUpdateProfileInfoDTO.LinkedInUrl;
            profile.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
