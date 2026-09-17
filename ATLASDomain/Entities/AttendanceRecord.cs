namespace ATLASDomain.Entities;

/// <summary>An immutable successful scan. A record is either present or does not exist.</summary>
public class AttendanceRecord
{
    public int AttendanceRecordId { get; set; }
    public Guid AttendanceSessionId { get; set; }
    public int StudentId { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string DeviceHash { get; set; } = null!;

    public AttendanceSession AttendanceSession { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
