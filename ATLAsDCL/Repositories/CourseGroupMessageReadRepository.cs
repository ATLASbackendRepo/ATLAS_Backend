using ATLAS.Domain.Entities;
using ATLAsDCL.Data;
using ATLASDomain.InterFaces.Repositories;

namespace ATLAsDCL.Repositories
{
    /// <summary>
    /// Course Group Message Read Repository.
    /// </summary>
    public class CourseGroupMessageReadRepository
        : GenericRepository<CourseGroupMessageRead>,
          ICourseGroupMessageReadRepository
    {
        public CourseGroupMessageReadRepository(AtLASDbContext context)
            : base(context)
        {
        }
    }
}
