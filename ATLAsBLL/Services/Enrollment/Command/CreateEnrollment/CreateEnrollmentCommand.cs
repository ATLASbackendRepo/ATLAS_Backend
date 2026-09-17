using ATLASDomain.DTO.Enrollment;
using MediatR;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment
{
    public record CreateEnrollmentCommand(string AcdimicID, BulkCreateEnrollmentDTO BulkCreateEnrollmentDTO1) : IRequest<bool>;
}