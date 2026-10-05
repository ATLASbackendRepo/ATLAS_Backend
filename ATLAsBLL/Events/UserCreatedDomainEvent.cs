// Application/Users/Events/UserCreatedEvent.cs
using ATLASDomain.Entities;
using MediatR;

namespace ATLAsBLL.Events
{
    /// <summary>
    /// User Created Event.
    /// </summary>
    public sealed class UserCreatedEvent : INotification
    {
        /// <summary>
        /// User Id.
        /// </summary>
        public int UserId { get; }
        /// <summary>
        /// Role.
        /// </summary>
        public enUserRole Role { get; }
        /// <summary>
        /// Full Name.
        /// </summary>
        public string FullName { get; }

        public UserCreatedEvent(int userId, enUserRole role, string fullName)
        {
            UserId = userId;
            Role = role;
            FullName = fullName;
        }
    }
}
