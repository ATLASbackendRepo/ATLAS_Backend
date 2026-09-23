using ATLAS.Domain.Entities;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories
{
    /// <summary>
    /// ICourse Group Message Repository.
    /// </summary>
    public interface ICourseGroupMessageRepository : IGenericRepository<CourseGroupMessage>
    {
        Task<IEnumerable<CourseGroupMessage>> GetByCourseOfferingAsync(int courseOfferingId, int page, int pageSize);
       
        Task SoftDeleteAsync(CourseGroupMessage message);
        Task TogglePinAsync(CourseGroupMessage message);
    }
}
