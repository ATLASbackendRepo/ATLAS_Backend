using System.Threading.Tasks;

namespace ATLASDomain.Services
{
    /// <summary>
    /// ICourse Access Service.
    /// </summary>
    public interface ICourseAccessService
    {
        Task<bool> CanSendAsync(int userId, int courseOfferingId);
        Task<bool> CanReadAsync(int userId, int courseOfferingId);
    }
}
