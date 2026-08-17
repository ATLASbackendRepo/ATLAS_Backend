namespace ATLAsBLL.Services.Notifications.DTOs;

public class NotificationDto
{
    public int NotificationID { get; set; }
    public int RecipientUserID { get; set; }
    public string? Type { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public int? RelatedMessageID { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}