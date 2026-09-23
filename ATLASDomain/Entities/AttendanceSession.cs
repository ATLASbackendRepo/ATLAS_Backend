namespace ATLASDomain.Entities;

/// <summary>
/// Attendance Session Status.
/// </summary>
public enum AttendanceSessionStatus
{
    Active = 1,
    Ended = 2,
    Expired = 3
}

/// <summary>One instructor-controlled attendance window for a course offering.</summary>
public class AttendanceSession
{
    /// <summary>
    /// Attendance Session Id.
    /// </summary>
    public Guid AttendanceSessionId { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Course Offering Id.
    /// </summary>
    public int CourseOfferingId { get; set; }
    /// <summary>
    /// Created By User Id.
    /// </summary>
    public int CreatedByUserId { get; set; }
    /// <summary>
    /// Started At.
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Expires At.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>
    /// Ended At.
    /// </summary>
    public DateTime? EndedAt { get; set; }
    /// <summary>
    /// Status.
    /// </summary>
    public AttendanceSessionStatus Status { get; set; } = AttendanceSessionStatus.Active;

    // A per-session secret used to sign short-lived QR payloads. It is never returned by the API.
    /// <summary>
    /// Signing Key.
    /// </summary>
    public string SigningKey { get; set; } = null!;

    /// <summary>
    /// Course Offering.
    /// </summary>
    public CourseOffering CourseOffering { get; set; } = null!;
    /// <summary>
    /// Created By User.
    /// </summary>
    public User CreatedByUser { get; set; } = null!;
    /// <summary>
    /// Records.
    /// </summary>
    public ICollection<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
}
