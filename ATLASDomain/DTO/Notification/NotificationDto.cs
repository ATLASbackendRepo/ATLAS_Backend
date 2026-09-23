namespace ATLAsBLL.Services.Notifications.DTOs;

/// <summary>
/// Notification Dto.
/// </summary>
public class NotificationDto
{
    /// <summary>
    /// Notification ID.
    /// </summary>
    public int NotificationID { get; set; }
    /// <summary>
    /// Recipient User ID.
    /// </summary>
    public int RecipientUserID { get; set; }
    /// <summary>
    /// Type.
    /// </summary>
    public string? Type { get; set; }
    /// <summary>
    /// Title.
    /// </summary>
    public string? Title { get; set; }
    /// <summary>
    /// Body.
    /// </summary>
    public string? Body { get; set; }
    /// <summary>
    /// Related Message ID.
    /// </summary>
    public int? RelatedMessageID { get; set; }
    /// <summary>
    /// Is Read.
    /// </summary>
    public bool IsRead { get; set; }
    /// <summary>
    /// Created At.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
