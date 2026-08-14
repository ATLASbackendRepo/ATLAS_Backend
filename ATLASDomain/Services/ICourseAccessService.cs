using System.Threading.Tasks;

namespace ATLASDomain.Services
{
    public interface ICourseAccessService
    {
        Task<bool> CanSendAsync(int userId, int courseOfferingId);
        Task<bool> CanReadAsync(int userId, int courseOfferingId);
    }
}
