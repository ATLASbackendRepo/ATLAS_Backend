
    using ATLAsDCL.UnitOfWork;
    using ATLASDomain.Entities;
    using global::ATLAsBLL.Events;
    using MediatR;
    using System.Threading;
    using System.Threading.Tasks;
using ATLAS.Domain.Entities;

namespace ATLAsBLL.Services.profile.Student 
{


    namespace ATLAsBLL.Services.Notifications
    {
        /// <summary>
        /// Create Student On User Created.
        /// </summary>
        public class CreateStudentOnUserCreated : INotificationHandler<StudentCreatedDomainEvent>
        {
            /// <summary>
            /// _unit Of Work.
            /// </summary>
            private readonly IUnitOfWork _unitOfWork;
            /// <summary>
            /// _publisher.
            /// </summary>
            private readonly IPublisher _publisher;

            public CreateStudentOnUserCreated(IUnitOfWork unitOfWork, IPublisher publisher)
            {
                _unitOfWork = unitOfWork;
                _publisher = publisher;
            }

            /// <summary>
            /// Handle.
            /// </summary>
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
