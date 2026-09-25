using ATLASDomain.DTO.Enrollment;
using MediatR;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment
{
    /// <summary>
    /// Create Enrollment Command.
    /// </summary>
    /// <summary>
    /// Create Enrollment Command.
    /// It is a command, so the transaction behavior wraps the insert and a
    /// failed enrollment never leaves a partial row behind.
    /// </summary>
    public record CreateEnrollmentCommand(
        string AcdimicID,
        BulkCreateEnrollmentDTO BulkCreateEnrollmentDTO1,
        int CreatedByUserId) : ICommand<bool>;
}
