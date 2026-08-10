using ATLASDomain.DTO.Student;
using ATLASDomain.Entities;

namespace ATLAsBLL.Mappings;

public static class StudentMappings
{
    public static StudentDTO ToDto(this Student student) => new()
    {
        StudentCode = student.StudentCode,
        Studentname = student.User.FullName,
        AcademicId = student.AcademicId,
        AcademicLevel = student.AcademicLevel
    };
}
