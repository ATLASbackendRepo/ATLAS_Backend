namespace ATLAsBLL.Services.CourseGroupMessage.DTOs;

public class CourseGroupMessageDto
{
    public int MessageID { get; set; }
    public int CourseOfferingID { get; set; }
    public int SenderUserID { get; set; }
    public string? SenderName { get; set; }
    public string? Content { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsPinned { get; set; }
    public bool IsDeleted { get; set; }
    public int ReadCount { get; set; }
}