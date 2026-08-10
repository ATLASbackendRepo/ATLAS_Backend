using ATLASDomain.DTO.Analysis;
using ATLASDomain.DTO.CourseOffring;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

public interface ICourseOfferingRepository : IGenericRepository<CourseOffering>
{
    Task<CourseOffering?> GetOfferingDetailsAsync(int offeringId);

    Task<IEnumerable<CourseOffering>> GetBySemesterAsync(int semesterId);

    Task<IEnumerable<CourseOffering>> GetByCourseAsync(int courseId);
    Task<IEnumerable<DoctorCourseOfferingDTO>> GetStaffMyCourseAsync(int staffId);
    Task<IEnumerable<CourseOfferingDTO>> GetMyCourseAsync(int id);

    Task<IEnumerable<CourseofferingDetailsDTO>> GetMyCourseDetailsAsync(int courseofferingId , int studentID   );




    Task<CourseAnalysisResponseDto> GetCourseAnalysisAsync(int courseOfferingId);
}
