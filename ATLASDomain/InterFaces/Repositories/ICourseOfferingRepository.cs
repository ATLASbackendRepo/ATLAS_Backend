using ATLASDomain.DTO.Analysis;
using ATLASDomain.DTO.CourseOffring;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// ICourse Offering Repository.
/// </summary>
public interface ICourseOfferingRepository : IGenericRepository<CourseOffering>
{
    /// <summary>
    /// Gets a single course offering with the details needed to display it.
    /// </summary>
    /// <param name="offeringId">Course offering identifier.</param>
    /// <returns>The offering, or null when it does not exist.</returns>
    Task<CourseOffering?> GetOfferingDetailsAsync(int offeringId);

    /// <summary>
    /// Gets every course offering that belongs to a semester.
    /// </summary>
    /// <param name="semesterId">Semester identifier.</param>
    /// <returns>The offerings of that semester.</returns>
    Task<IEnumerable<CourseOffering>> GetBySemesterAsync(int semesterId);

    /// <summary>
    /// Gets every course offering of a course, across all semesters.
    /// </summary>
    /// <param name="courseId">Course identifier.</param>
    /// <returns>The offerings of that course.</returns>
    Task<IEnumerable<CourseOffering>> GetByCourseAsync(int courseId);
    /// <summary>
    /// Gets the offerings a staff member teaches, shaped for the staff "my
    /// courses" screen.
    /// </summary>
    /// <param name="staffId">Staff user identifier.</param>
    /// <returns>The offerings the staff member teaches.</returns>
    Task<IEnumerable<DoctorCourseOfferingDTO>> GetStaffMyCourseAsync(int staffId);
    /// <summary>
    /// Gets the offerings a student is enrolled in, shaped for the student "my
    /// courses" screen.
    /// </summary>
    /// <param name="id">Student user identifier.</param>
    /// <returns>The offerings the student is enrolled in.</returns>
    Task<IEnumerable<CourseOfferingDTO>> GetMyCourseAsync(int id);
    /// <summary>
    /// Gets a course offering by its identifier.
    /// </summary>
    /// <param name="id">Course offering identifier.</param>
    /// <returns>The offering, or null when it does not exist.</returns>
    Task<CourseOffering?> GetCourseOFferingByID(int id);
    /// <summary>
    /// Gets the detailed view of an offering for one student, including the
    /// modules and the grades that student has received.
    /// </summary>
    /// <param name="courseofferingId">Course offering identifier.</param>
    /// <param name="studentID">Student user identifier.</param>
    /// <returns>The offering details for that student.</returns>
    Task<IEnumerable<CourseofferingDetailsDTO>> GetMyCourseDetailsAsync(int courseofferingId , int studentID   );

    /// <summary>
    /// Gets the detailed view of an offering used by staff.
    /// </summary>
    /// <param name="courseOfferingId">Course offering identifier.</param>
    /// <returns>The offering details.</returns>
    Task<IEnumerable<CourseofferingDetailsDTO>> GetDetailsCourseAsync(
        int courseOfferingId);
    


    /// <summary>
    /// Gets the aggregate figures shown on the course analysis screen.
    /// </summary>
    /// <param name="courseOfferingId">Course offering identifier.</param>
    /// <returns>The analysis figures for the offering.</returns>
    Task<CourseAnalysisResponseDto> GetCourseAnalysisAsync(int courseOfferingId);
}
