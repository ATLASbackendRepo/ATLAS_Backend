using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ATLAsBLL.Features.Students.Commands.CreateStudentsRange
{
    // يستقبل قائمة تحتوي على بيانات المستخدمين والطلاب معاً
    public record CreateStudentsRangeCommand(string filePath) : ICommand<bool>;

    
}
