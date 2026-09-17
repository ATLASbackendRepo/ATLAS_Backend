using ATLASDomain.DTO.Enrollment;
using MediatR;
using System.Collections.Generic;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange
{
    public record CreateEnrollmentsRangeCommand(BulkCreateEnrollmentDTO BulkCreateEnrollment) : ICommand<bool>;

   
}