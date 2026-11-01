using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Events;
using ATLAsBLL.Services.Common;
using MediatR;

namespace ATLAsBLL.Features.Students.Commands.CreateStudent
{
    /// <summary>
    /// Create Student Command Handler.
    /// Handles single-student account creation with server-generated temporary credentials (BE-07 / BE-08).
    /// </summary>
    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, CreateStudentResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public CreateStudentCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        /// <summary>
        /// Handles student creation. Ignores any client-supplied initial password, generates
        /// a unique cryptographically secure temporary password, and returns a protected one-row export.
        /// </summary>
        public async Task<CreateStudentResult> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            var academicId = request.CreateStudentDTO.AcademicId?.Trim() ?? string.Empty;

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
                FullName = request.CreateStudentDTO.FullName?.Trim() ?? string.Empty,
                AcademicId = academicId,
                Role = enUserRole.Student,
                IsActive = true,
                MustChangePassword = true,
                TemporaryPasswordExpiresAt = expiresAt,
                InstitutionID = request.CreateStudentDTO.InstitutionId,
                DepartmentId = request.CreateStudentDTO.DepartmentId,
                Password = BCrypt.Net.BCrypt.HashPassword(temporaryPassword)
            };

            var student = new Student
            {
                AcademicId = academicId,
                AcademicLevel = request.CreateStudentDTO.AcademicLevel,
                DepartmentId = request.CreateStudentDTO.DepartmentId,
                InstitutionID = request.CreateStudentDTO.InstitutionId,
                User = user,
                IsActive = true
            };

            await _unitOfWork.Students.AddAsync(student, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new StudentCreatedDomainEvent(student.StudentId), cancellationToken);

            // Decision 1A: One-row protected spreadsheet export streamed to SAdmin
            var records = new[]
            {
                new StudentCredentialRecord(
                    student.AcademicId,
                    user.FullName,
                    temporaryPassword,
                    expiresAt,
                    "Student")
            };
            var excelBytes = CredentialExportService.GenerateCredentialsWorkbook(records);

            return new CreateStudentResult(
                student.StudentId,
                student.AcademicId,
                user.FullName,
                temporaryPassword,
                expiresAt,
                excelBytes);
        }
    }
}