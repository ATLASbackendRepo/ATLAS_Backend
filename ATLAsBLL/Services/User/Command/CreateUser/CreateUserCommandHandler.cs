using ATLAsBLL.Events;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ATLAsBLL.Features.Users.Commands.CreateUser
{
    /// <summary>
    /// Create User Command Handler.
    /// </summary>
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// _publisher.
        /// </summary>
        private readonly IPublisher _publisher;
        public CreateUserCommandHandler(
           IUnitOfWork unitOfWork,
           IPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }
    public const int MinimumPasswordLength = 8;
        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {



            var academicId = request.create_StaffDto.AcademicId;

            // The academic id is the login identity; a second account reusing it
            // would make logins and imports resolve to an arbitrary row.
            var academicIdTaken = await _unitOfWork.Users
                .AnyAsync(existing => existing.AcademicId == academicId);

            if (academicIdTaken)
            {
                throw new InvalidOperationException(
                    $"Academic ID '{academicId}' is already in use.");
            }


             if (string.IsNullOrWhiteSpace(request.create_StaffDto.Password))
                throw new InvalidDataException("New password is required.");
            if (request.create_StaffDto.Password.Length < MinimumPasswordLength)
                throw new InvalidDataException($"New password must be at least {MinimumPasswordLength} characters long.");

            // The initial password is the one the administrator chooses; the
            // account must replace it at the first login.
            var initialPassword = request.create_StaffDto.Password;
           


            var user = new User
            {
                FullName = request.create_StaffDto.FullName,
                Role = request.create_StaffDto.Role
                ,
                AcademicId = request.create_StaffDto.AcademicId
                ,DepartmentId = request.create_StaffDto.DepartmentId
                ,InstitutionID = request.create_StaffDto.InstituteId       
                ,IsActive = true
                ,MustChangePassword = true
            };

                user.Password = BCrypt.Net.BCrypt.HashPassword(initialPassword);

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
