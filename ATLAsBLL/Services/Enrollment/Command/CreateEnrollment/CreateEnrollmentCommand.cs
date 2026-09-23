using ATLASDomain.DTO.Enrollment;
using MediatR;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment
{
    /// <summary>
    /// Create Enrollment Command.
    /// </summary>
    public record CreateEnrollmentCommand(string AcdimicID, BulkCreateEnrollmentDTO BulkCreateEnrollmentDTO1) : IRequest<bool>;
}
