namespace ATLASDomain.Entities;

public enum AttendanceSessionStatus
{
    Active = 1,
    Ended = 2,
    Expired = 3
}

/// <summary>One instructor-controlled attendance window for a course offering.</summary>
public class AttendanceSession
{
    public Guid AttendanceSessionId { get; set; } = Guid.NewGuid();
    public int CourseOfferingId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public AttendanceSessionStatus Status { get; set; } = AttendanceSessionStatus.Active;

    // A per-session secret used to sign short-lived QR payloads. It is never returned by the API.
    public string SigningKey { get; set; } = null!;

    public CourseOffering CourseOffering { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
}
