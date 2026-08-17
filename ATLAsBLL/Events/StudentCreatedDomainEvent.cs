using MediatR;


namespace ATLAsBLL.Events
{
    public sealed class StudentCreatedDomainEvent : INotification
    {
        public int StudentId { get; }

        public StudentCreatedDomainEvent(int studentId)
        {
            StudentId = studentId;
        }
    }
}
