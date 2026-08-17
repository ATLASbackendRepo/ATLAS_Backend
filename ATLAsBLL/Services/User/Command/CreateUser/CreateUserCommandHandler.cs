using ATLAsBLL.Events;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPublisher _publisher;
        public CreateUserCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                FullName = request.FullName,
                Role = request.Role
            };

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

     
                await _publisher.Publish(
                    new UserCreatedEvent(user.UserId, user.Role, user.FullName),
                    cancellationToken
                );
            

            return user.UserId;
        }
    }
}