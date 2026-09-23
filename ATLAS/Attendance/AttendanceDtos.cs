namespace ATLAS.Attendance;

/// <summary>
/// Start Attendance Session Request.
/// </summary>
public sealed record StartAttendanceSessionRequest(int CourseOfferingId, int DurationMinutes = 60);
/// <summary>
/// Attendance Qr Dto.
/// </summary>
public sealed record AttendanceQrDto(Guid SessionId, string Token, DateTime ValidUntil, DateTime SessionExpiresAt);
/// <summary>
/// Attendance Session Dto.
/// </summary>
public sealed record AttendanceSessionDto(Guid SessionId, int CourseOfferingId, DateTime StartedAt, DateTime ExpiresAt, DateTime? EndedAt, string Status, int PresentCount);
/// <summary>
/// Attendance Scan Request.
/// </summary>
public sealed record AttendanceScanRequest(string QrToken, string DeviceId);
/// <summary>
/// Attendance Scan Result.
/// </summary>
public sealed record AttendanceScanResult(Guid SessionId, DateTime RecordedAt, string Message);
/// <summary>
/// Attendance Report Row Dto.
/// </summary>
public sealed record AttendanceReportRowDto(int StudentId, string StudentCode, string StudentName, string Status, DateTime? RecordedAt);

/// <summary>
/// Attendance Report Dto.
/// </summary>
public sealed record AttendanceReportDto(Guid SessionId, int CourseOfferingId, DateTime StartedAt, DateTime ExpiresAt, DateTime? EndedAt, string Status, IReadOnlyList<AttendanceReportRowDto> Students);
