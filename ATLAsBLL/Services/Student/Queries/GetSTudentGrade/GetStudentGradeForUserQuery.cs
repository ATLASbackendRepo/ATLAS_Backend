using ATLASDomain.DTO.Student;

namespace ATLAsBLL.Services.Student.Queries.GetSTudentGrade;

public record GetStudentGradeForUserQuery(int UserId, int CourseOfferingId, int ModuleId) : IQuery<StudentGradeDto?>;
