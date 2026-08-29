using ATLASDomain.Entities;


namespace ATLASDomain.InterFaces
{
    public interface INotificationProvider
    {
        string Name { get; }
        Task SendAsync(SecurityEvent securityEvent, CancellationToken cancellationToken);
    }
}
