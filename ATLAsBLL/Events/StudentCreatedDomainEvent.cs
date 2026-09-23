using MediatR;


namespace ATLAsBLL.Events
{
    /// <summary>
    /// Student Created Domain Event.
    /// </summary>
    public sealed class StudentCreatedDomainEvent : INotification
    {
        /// <summary>
        /// Student Id.
        /// </summary>
        public int StudentId { get; }

        public StudentCreatedDomainEvent(int studentId)
        {
            StudentId = studentId;
        }
    }
}
