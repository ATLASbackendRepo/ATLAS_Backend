using ATLASDomain.DTO.Student;

namespace ATLAsBLL.Services.Student.Queries.GetSTudentGrade;

/// <summary>
/// Get Student Grade For User Query.
/// </summary>
public record GetStudentGradeForUserQuery(int UserId, int CourseOfferingId, int ModuleId) : IQuery<StudentGradeDto?>;
