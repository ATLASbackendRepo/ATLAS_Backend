using ATLASDomain.Entities;
using ATLASDomain.DTO.Student;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// IStudent Repository.
/// </summary>
public interface IStudentRepository : IGenericRepository<Student>
{
    /// <summary>
    /// Gets a student together with the user account behind it.
    /// </summary>
    /// <param name="studentCode">Student code to match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The student, or null when no student has that code.</returns>
    Task<Student?> GetStudentWithUserAsync(string studentCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a filtered, paged list of students.
    /// </summary>
    /// <param name="filter">Paging and filtering criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching students.</returns>
    Task<List<StudentDTO>> GetAllStudents(StudentFilterDTO filter, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets a student together with its department.
    /// </summary>
    /// <param name="studentId">Student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The student, or null when it does not exist.</returns>
    Task<Student?> GetStudentWithDepartmentAsync(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a student together with its enrollments.
    /// </summary>
    /// <param name="studentId">Student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The student, or null when it does not exist.</returns>
    Task<Student?> GetStudentWithEnrollmentsAsync(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a student code is already taken.
    /// </summary>
    /// <param name="studentCode">Student code to test.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when a student already uses that code.</returns>
    Task<bool> StudentCodeExistsAsync(string studentCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a student by academic number.
    /// </summary>
    /// <param name="academicID">Academic number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The student, or null when no student has that number.</returns>
    Task<Student?> GetStudentByAcademicIDAsync(string academicID, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the student record behind a user account.
    /// </summary>
    /// <param name="UserID">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The student, or null when the user is not a student.</returns>
    Task<Student?> GetStudentByUserIDAsync(int UserID, CancellationToken cancellationToken = default);

   /// <summary>
   /// Gets a student by student code.
   /// </summary>
   /// <param name="studentCode">Student code to match.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>The student, or null when no student has that code.</returns>
   Task<Student?> GetStudentByCodeAsync(string studentCode, CancellationToken cancellationToken = default);
   /// <summary>
   /// Gets every student in an academic level.
   /// </summary>
   /// <param name="level">Academic level.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>The students in that level.</returns>
   Task<IEnumerable<Student>> GetAllStudentWithLevelAsync(int level, CancellationToken cancellationToken = default);
   /// <summary>
   /// Gets every student enrolled in a course offering.
   /// </summary>
   /// <param name="courseOfferingID">Course offering identifier.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>The enrolled students.</returns>
   Task <IEnumerable<Student>> GetAllStudentWithCoursefferingAsync( int courseOfferingID, CancellationToken cancellationToken = default);
   /// <summary>
   /// Gets every student enrolled in a course offering together with their grade
   /// for one assessment module.
   /// </summary>
   /// <param name="courseOfferingID">Course offering identifier.</param>
   /// <param name="modelId">Assessment module identifier.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>The enrolled students and their grades.</returns>
   Task <IEnumerable<StudentGradeDto>> GetAllStudentWithModeAsync( int courseOfferingID,int modelId, CancellationToken cancellationToken = default);
   /// <summary>
   /// Gets one student's grade for a single assessment module.
   /// </summary>
   /// <param name="courseOfferingID">Course offering identifier.</param>
   /// <param name="modelId">Assessment module identifier.</param>
   /// <param name="StudentCode">Student code.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>The student's grade, or null when none has been recorded.</returns>
   Task <StudentGradeDto?> GetStudentWithModeAsync( int courseOfferingID,int modelId , string StudentCode, CancellationToken cancellationToken = default);
}

