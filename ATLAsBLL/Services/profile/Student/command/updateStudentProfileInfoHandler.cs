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
    public class updateStudentProfileInfoHandler : IRequestHandler<updateStudentProfileInfoCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public updateStudentProfileInfoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(
            updateStudentProfileInfoCommand request,
            CancellationToken cancellationToken)
        {
            var student = await _unitOfWork.Students.GetStudentByUserIDAsync(request.UserId);

            if(student == null) 
                return false;

            var profile = await _unitOfWork.StudentProfile.GetByUserIdAsync(student.StudentId);

            if (profile == null)
                return false;

            // Update the profile with the new information
            profile.Student.User.FullName = request.StudentUpdateProfileInfoDTO.FullName;
            profile.Faculty = request.StudentUpdateProfileInfoDTO.Faculty;
            profile.SectionNumber = request.StudentUpdateProfileInfoDTO.SectionNumber;
            profile.LinkedInUrl = request.StudentUpdateProfileInfoDTO.LinkedInUrl;

            _unitOfWork.StudentProfile.Update(profile);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
