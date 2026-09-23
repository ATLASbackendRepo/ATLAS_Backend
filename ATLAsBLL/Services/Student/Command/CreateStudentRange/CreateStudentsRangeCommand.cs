using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ATLAsBLL.Features.Students.Commands.CreateStudentsRange
{
    // يستقبل قائمة تحتوي على بيانات المستخدمين والطلاب معاً
    /// <summary>
    /// Create Students Range Command.
    /// </summary>
    public record CreateStudentsRangeCommand(string filePath) : ICommand<bool>;

    
}
