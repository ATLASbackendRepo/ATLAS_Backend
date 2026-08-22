using ATLAsBLL.Services.profile.Staff.Query;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace ATLAsBLL.Services.profile.Student.Query
{
    public class GetStudentProfileByUserIdQueryHandler : IRequestHandler<GetStudentProfileByUserIdQuery, StudentProfileResponseDTO?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStudentProfileByUserIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<StudentProfileResponseDTO?> Handle(
            GetStudentProfileByUserIdQuery request,
            CancellationToken cancellationToken)
        {
            var student = await _unitOfWork.Students.GetStudentByUserIDAsync(request.UserId);
            if (student == null)
                return null;
            var profile = await _unitOfWork.StudentProfile.GetByUserIdAsync(student.StudentId);

            if (profile == null)
                return null;

            return new StudentProfileResponseDTO
            {
                StudentProfileID = profile.StudentProfileID,
                Faculty = profile.Faculty ?? string.Empty,
                FullName = profile.Student?.User?.FullName ?? string.Empty,
             

                LinkedInUrl = profile.LinkedInUrl,
                SectionNumber = profile.SectionNumber,
                QRCodeUrl = profile.QRCodeUrl
            };
        }
    }
    }
