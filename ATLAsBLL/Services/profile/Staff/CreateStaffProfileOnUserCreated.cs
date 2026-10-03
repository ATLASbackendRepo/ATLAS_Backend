using ATLAS.Domain.Entities;
using ATLAsBLL.Events;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.profile.Staff
{
    /// <summary>
    /// Create Staff Profile On User Created.
    /// </summary>
    public class CreateStaffProfileOnUserCreated : INotificationHandler<UserCreatedEvent>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public CreateStaffProfileOnUserCreated(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task Handle(UserCreatedEvent User, CancellationToken cancellationToken)
        {
            if (User.Role != enUserRole.TeachingAssistant && User.Role != enUserRole.Doctor)
                return;

            var staffProfile = new StaffProfile
            {
                UserID = User.UserId,
                Role = User.Role.ToString(),
              
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.StaffProfile.AddAsync(staffProfile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
