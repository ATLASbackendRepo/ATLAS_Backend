using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Events;
using System.Security.Cryptography;

namespace ATLAsBLL.Features.Students.Commands.CreateStudent
{
    /// <summary>
    /// Create Student Command Handler.
    /// </summary>
    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, int>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        /// <summary>
        /// _publisher.
        /// </summary>
        private readonly IPublisher _publisher;
          public const int MinimumPasswordLength = 8;
        public CreateStudentCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<int> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            

             if (string.IsNullOrWhiteSpace(request.CreateStudentDTO.password))
                throw new InvalidDataException("New password is required.");
            if (request.CreateStudentDTO.password.Length < MinimumPasswordLength)
                throw new InvalidDataException($"New password must be at least {MinimumPasswordLength} characters long.");
           

            var academicId = request.CreateStudentDTO.AcademicId;

            // The academic id is the login identity; a second account reusing it
            // would make logins and imports resolve to an arbitrary row.
            var academicIdTaken = await _unitOfWork.Users
                .AnyAsync(existing => existing.AcademicId == academicId, cancellationToken);

            if (academicIdTaken)
            {
                throw new InvalidOperationException(
                    $"Academic ID '{academicId}' is already in use.");
            }

            var user = new User
            {
                FullName = request.CreateStudentDTO.FullName
                ,
                AcademicId = request.CreateStudentDTO.AcademicId 
                , Role = enUserRole.Student,
                IsActive =true,
                MustChangePassword = true,
                InstitutionID = request.CreateStudentDTO.InstitutionId,
                DepartmentId = request.CreateStudentDTO.DepartmentId

            };

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.CreateStudentDTO.password);

         
            var student = new Student
            {
                AcademicId = request.CreateStudentDTO.AcademicId,
                AcademicLevel = request.CreateStudentDTO.AcademicLevel,
                DepartmentId = request.CreateStudentDTO.DepartmentId,
                InstitutionID = request.CreateStudentDTO.InstitutionId,
                User = user 
                ,IsActive = true
            };

           
            await _unitOfWork.Students.AddAsync(student, cancellationToken);

            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            
            await _publisher.Publish(new StudentCreatedDomainEvent(student.StudentId), cancellationToken);

            return student.StudentId;
        }
    }
}
