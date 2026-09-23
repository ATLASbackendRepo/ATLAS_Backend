namespace ATLASDomain.Entities;

/// <summary>An immutable successful scan. A record is either present or does not exist.</summary>
public class AttendanceRecord
{
    /// <summary>
    /// Attendance Record Id.
    /// </summary>
    public int AttendanceRecordId { get; set; }
    /// <summary>
    /// Attendance Session Id.
    /// </summary>
    public Guid AttendanceSessionId { get; set; }
    /// <summary>
    /// Student Id.
    /// </summary>
    public int StudentId { get; set; }
    /// <summary>
    /// Recorded At.
    /// </summary>
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Device Hash.
    /// </summary>
    public string DeviceHash { get; set; } = null!;

    /// <summary>
    /// Attendance Session.
    /// </summary>
    public AttendanceSession AttendanceSession { get; set; } = null!;
    /// <summary>
    /// Student.
    /// </summary>
    public Student Student { get; set; } = null!;
}
