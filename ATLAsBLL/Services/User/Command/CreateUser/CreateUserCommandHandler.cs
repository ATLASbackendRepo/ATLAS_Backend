using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ATLAsBLL.Events;
using ATLAsBLL.Services.Common;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;

namespace ATLAsBLL.Features.Users.Commands.CreateUser
{
    /// <summary>
    /// Create User Command Handler.
    /// Handles single staff account creation with server-generated temporary credentials (BE-07 / BE-08).
    /// </summary>
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
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

        /// <summary>
        /// Handles staff user creation. Ignores client-supplied initial password, generates
        /// a unique cryptographically secure temporary password, and returns a protected one-row export.
        /// </summary>
        public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var academicId = request.create_StaffDto.AcademicId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(academicId))
                throw new InvalidDataException("Academic ID is required.");

            // The academic id is the login identity; a second account reusing it
            // would make logins and imports resolve to an arbitrary row.
            var academicIdTaken = await _unitOfWork.Users
                .AnyAsync(existing => existing.AcademicId == academicId, cancellationToken);

            if (academicIdTaken)
            {
                throw new InvalidOperationException(
                    $"Academic ID '{academicId}' is already in use.");
            }

            // BE-08: Stop trusting client-supplied initial passwords.
            // Generate a cryptographically secure 24-character temporary password server-side.
            var temporaryPassword = CredentialExportService.GenerateTemporaryPassword();
            var expiresAt = DateTime.UtcNow.AddHours(72);

            var user = new User
            {
                FullName = request.create_StaffDto.FullName?.Trim() ?? string.Empty,
                Role = request.create_StaffDto.Role,
                AcademicId = academicId,
                DepartmentId = request.create_StaffDto.DepartmentId,
                InstitutionID = request.create_StaffDto.InstituteId,
                IsActive = true,
                MustChangePassword = true,
                TemporaryPasswordExpiresAt = expiresAt,
                Password = BCrypt.Net.BCrypt.HashPassword(temporaryPassword)
            };

            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(
                new UserCreatedEvent(user.UserId, user.Role, user.FullName),
                cancellationToken
            );

            // Decision 1A: One-row protected spreadsheet export streamed to SAdmin
            var records = new[]
            {
                new StudentCredentialRecord(
                    user.AcademicId,
                    user.FullName,
                    temporaryPassword,
                    expiresAt,
                    user.Role.ToString())
            };
            var excelBytes = CredentialExportService.GenerateCredentialsWorkbook(records);

            return new CreateUserResult(
                user.UserId,
                user.AcademicId,
                user.FullName,
                user.Role,
                temporaryPassword,
                expiresAt,
                excelBytes);
        }
    }
}