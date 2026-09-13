using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Events;

namespace ATLAsBLL.Features.Students.Commands.CreateStudent
{
    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public CreateStudentCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<int> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            

            var user = new User
            {
                FullName = request.CreateStudentDTO.FullName
                ,
                AcademicId = request.CreateStudentDTO.AcademicId 
                , Role = enUserRole.Student,
                IsActive =true

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
