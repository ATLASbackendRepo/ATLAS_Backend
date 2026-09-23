namespace ATLAsBLL.Services.CourseGroupMessage.DTOs;

/// <summary>
/// Course Group Message Dto.
/// </summary>
public class CourseGroupMessageDto
{
    /// <summary>
    /// Message ID.
    /// </summary>
    public int MessageID { get; set; }
    /// <summary>
    /// Course Offering ID.
    /// </summary>
    public int CourseOfferingID { get; set; }
    /// <summary>
    /// Sender User ID.
    /// </summary>
    public int SenderUserID { get; set; }
    /// <summary>
    /// Sender Name.
    /// </summary>
    public string? SenderName { get; set; }
    /// <summary>
    /// Content.
    /// </summary>
    public string? Content { get; set; }
    /// <summary>
    /// Sent At.
    /// </summary>
    public DateTime SentAt { get; set; }
    /// <summary>
    /// Is Pinned.
    /// </summary>
    public bool IsPinned { get; set; }
    /// <summary>
    /// Is Deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
    /// <summary>
    /// Read Count.
    /// </summary>
    public int ReadCount { get; set; }
}
