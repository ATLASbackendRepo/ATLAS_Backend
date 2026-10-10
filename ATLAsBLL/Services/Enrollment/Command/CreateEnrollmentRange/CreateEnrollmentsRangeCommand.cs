using ATLASDomain.DTO.Enrollment;
using MediatR;
using System.IO;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange
{
    /// <summary>
    /// Create Enrollments Range Command.
    /// Handles Excel file bulk student enrollment into a specified CourseOffering.
    /// </summary>
    public record CreateEnrollmentsRangeCommand(
        Stream FileStream,
        string FileName,
        BulkCreateEnrollmentDTO BulkCreateEnrollment,
        int CreatedByUserId
    ) : IRequest<BulkEnrollmentResultDTO>, IWriteCommand;
}
