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
        public CreateUserCommandHandler(
           IUnitOfWork unitOfWork,
           IPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                FullName = request.create_StaffDto.FullName,
                Role = request.create_StaffDto.Role
                ,
                AcademicId = request.create_StaffDto.AcademicId
                ,DepartmentId = request.create_StaffDto.DepartmentId
                ,InstitutionID = request.create_StaffDto.InstituteId       
                ,IsActive = true
            };

                user.Password = BCrypt.Net.BCrypt.HashPassword("12345678");
               
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