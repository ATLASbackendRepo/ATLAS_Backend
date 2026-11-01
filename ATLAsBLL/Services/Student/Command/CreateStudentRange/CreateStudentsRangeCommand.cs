using ATLAsBLL.CQRS;
using ATLAsBLL.Services.Common;
using MediatR;

namespace ATLAsBLL.Features.Students.Commands.CreateStudentsRange
{
    /// <summary>
    /// Create Students Range Command.
    /// </summary>
    /// <remarks>
    /// The handler manages its own transaction
    /// (<see cref="ISelfManagedTransactionCommand{TResponse}"/>) so the file is
    /// parsed and every password is hashed before the transaction opens.
    /// </remarks>
    public record CreateStudentsRangeCommand(string filePath) : ISelfManagedTransactionCommand<StudentImportResult>;
}