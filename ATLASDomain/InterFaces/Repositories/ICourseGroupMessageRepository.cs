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
        /// <summary>
        /// Gets a page of the messages posted to a course offering's group chat,
        /// newest first.
        /// </summary>
        /// <param name="courseOfferingId">Course offering identifier.</param>
        /// <param name="page">One-based page number.</param>
        /// <param name="pageSize">Number of messages per page.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The requested page of messages.</returns>
        Task<IEnumerable<CourseGroupMessage>> GetByCourseOfferingAsync(int courseOfferingId, int page, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks a message as deleted without removing the row.
        /// </summary>
        /// <param name="message">The message to soft delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SoftDeleteAsync(CourseGroupMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Toggles whether a message is pinned to the top of the chat.
        /// </summary>
        /// <param name="message">The message to pin or unpin.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task TogglePinAsync(CourseGroupMessage message, CancellationToken cancellationToken = default);
    }
}
