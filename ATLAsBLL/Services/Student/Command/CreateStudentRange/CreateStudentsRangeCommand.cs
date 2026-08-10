using MediatR;
using System.Collections.Generic;

namespace ATLAsBLL.Features.Students.Commands.CreateStudentsRange
{
    // يستقبل قائمة تحتوي على بيانات المستخدمين والطلاب معاً
    public record CreateStudentsRangeCommand(List<StudentItemDto> Students) : ICommand<List<int>>;

    public record StudentItemDto(
        string FullName,
        string Password,
        string StudentCode,
        string AcademicId,
        int AcademicLevel,
        int DepartmentId
    );
}
