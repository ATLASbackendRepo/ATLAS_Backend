using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ATLAsBLL.Features.Students.Commands.CreateStudentsRange
{
    // يستقبل قائمة تحتوي على بيانات المستخدمين والطلاب معاً
    /// <summary>
    /// Create Students Range Command.
    /// </summary>
    /// <remarks>
    /// The handler manages its own transaction
    /// (<see cref="ISelfManagedTransactionCommand{TResponse}"/>) so the file is
    /// parsed and every password is hashed before the transaction opens.
    /// </remarks>
    public record CreateStudentsRangeCommand(string filePath) : ISelfManagedTransactionCommand<bool>;

    
}
