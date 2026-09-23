using ATLASDomain.Entities;


namespace ATLASDomain.InterFaces
{
    /// <summary>
    /// INotification Provider.
    /// </summary>
    public interface INotificationProvider
    {
        string Name { get; }
        Task SendAsync(SecurityEvent securityEvent, CancellationToken cancellationToken);
    }
}
