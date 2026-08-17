
    using ATLAsDCL.UnitOfWork;
    using ATLASDomain.Entities;
    using global::ATLAsBLL.Events;
    using MediatR;
using ATLASDomain.Entities;
    using System.Threading;
    using System.Threading.Tasks;
using ATLAS.Domain.Entities;

namespace ATLAsBLL.Services.profile.Student 
{


    namespace ATLAsBLL.Services.Notifications
    {
        public class CreateStudentOnUserCreated : INotificationHandler<StudentCreatedDomainEvent>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IPublisher _publisher;

            public CreateStudentOnUserCreated(IUnitOfWork unitOfWork, IPublisher publisher)
            {
                _unitOfWork = unitOfWork;
                _publisher = publisher;
            }

            public async Task Handle(StudentCreatedDomainEvent student, CancellationToken cancellationToken)
            {

                var studentProfile = new StudentProfile

                {
                    StudentID = student.StudentId ,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.StudentProfile. AddAsync(studentProfile);
                await _unitOfWork.SaveChangesAsync(cancellationToken); // عشان ناخد الـ StudentID المولّد

               
            }
        }
    }
}
