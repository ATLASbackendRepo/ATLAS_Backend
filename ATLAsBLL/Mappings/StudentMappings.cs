using ATLASDomain.DTO.Student;
using ATLASDomain.Entities;

namespace ATLAsBLL.Mappings;

/// <summary>
/// Student Mappings.
/// </summary>
public static class StudentMappings
{
    /// <summary>
    /// To Dto.
    /// </summary>
    public static StudentDTO ToDto(this Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(student.User);

        return new()
        {
            StudentCode = student.StudentCode,
            StudentName = student.User.FullName ?? string.Empty,
            AcademicId = student.AcademicId ?? string.Empty,
            AcademicLevel = student.AcademicLevel
        };
    }
}
