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

            user.Password = BCrypt.Net.BCrypt.HashPassword("12345678");

         
            var student = new Student
            {
                AcademicId = request.CreateStudentDTO.AcademicId,
                AcademicLevel = request.CreateStudentDTO.AcademicLevel,
                DepartmentId = request.CreateStudentDTO.DepartmentId,
                InstitutionID = request.CreateStudentDTO.InstitutionId,
                User = user 
                ,IsActive = true
            };

           
            await _unitOfWork.Students.AddAsync(student);

            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            
            await _publisher.Publish(new StudentCreatedDomainEvent(student.StudentId), cancellationToken);

            return student.StudentId;
        }
    }
}
