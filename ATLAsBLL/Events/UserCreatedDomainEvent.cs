// Application/Users/Events/UserCreatedEvent.cs
using ATLASDomain.Entities;
using MediatR;
namespace ATLAsBLL.Events
{
    public sealed class UserCreatedEvent : INotification
    {
        public int UserId { get; }
        public enUserRole Role { get; }
        public string FullName { get; }

        public UserCreatedEvent(int userId, enUserRole role, string fullName)
        {
            UserId = userId;
            Role = role;
            FullName = fullName;
        }
    }
}