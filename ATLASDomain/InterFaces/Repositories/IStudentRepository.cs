using ATLASDomain.Entities;
using ATLASDomain.DTO.Student;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

public interface IStudentRepository : IGenericRepository<Student>
{
    Task<Student?> GetStudentWithUserAsync(int studentId);

    Task<List<Student>> GetAllStudents();
    Task<Student?> GetStudentWithDepartmentAsync(int studentId);

    Task<Student?> GetStudentWithEnrollmentsAsync(int studentId);

    Task<bool> StudentCodeExistsAsync(string studentCode);

    Task<Student?> GetStudentByAcademicIDAsync(string academicID);
    Task<Student?> GetStudentByUserIDAsync(int UserID);

   Task<Student?> GetStudentByCodeAsync(string studentCode);
   Task<IEnumerable<Student>> GetAllStudentWithLevelAsync(int level);
   Task <IEnumerable<Student>> GetAllStudentWithCoursefferingAsync( int courseOfferingID);
   Task <IEnumerable<StudentGradeDto>> GetAllStudentWithModeAsync( int courseOfferingID,int modelId);
   Task <StudentGradeDto> GetStudentWithModeAsync( int courseOfferingID,int modelId , string StudentCode);
}

