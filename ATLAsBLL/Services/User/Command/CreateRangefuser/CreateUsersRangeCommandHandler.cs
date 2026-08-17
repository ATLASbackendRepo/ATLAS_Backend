using ATLAsBLL.Events;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;

namespace ATLAsBLL.Features.Users.Commands.CreateUsersRange
{
    public class CreateUsersRangeCommandHandler : IRequestHandler<CreateUsersRangeCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public CreateUsersRangeCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<string> Handle(CreateUsersRangeCommand request, CancellationToken cancellationToken)
        {  
            var usersEntities = new List<User>();

            foreach (var u in request.Users)
            {
                var user = new User
                {
                    FullName = u.FullName,
                    Role = u.Role
                };

                user.Password = BCrypt.Net.BCrypt.HashPassword(u.Password);
                usersEntities.Add(user);
            }

            
            await _unitOfWork.Users.AddRangeAsync(usersEntities);

            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var user in usersEntities)
            {
                await _publisher.Publish(
                    new UserCreatedEvent(user.UserId, user.Role, user.FullName),
                    cancellationToken
                );
            }

           
            return "inserted successfully";
        }
    }
}