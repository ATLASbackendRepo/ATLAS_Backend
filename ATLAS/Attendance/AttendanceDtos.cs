namespace ATLAS.Attendance;

public sealed record StartAttendanceSessionRequest(int CourseOfferingId, int DurationMinutes = 60);
public sealed record AttendanceQrDto(Guid SessionId, string Token, DateTime ValidUntil, DateTime SessionExpiresAt);
public sealed record AttendanceSessionDto(Guid SessionId, int CourseOfferingId, DateTime StartedAt, DateTime ExpiresAt, DateTime? EndedAt, string Status, int PresentCount);
public sealed record AttendanceScanRequest(string QrToken, string DeviceId);
public sealed record AttendanceScanResult(Guid SessionId, DateTime RecordedAt, string Message);
public sealed record AttendanceReportRowDto(int StudentId, string StudentCode, string StudentName, string Status, DateTime? RecordedAt);

public sealed record AttendanceReportDto(Guid SessionId, int CourseOfferingId, DateTime StartedAt, DateTime ExpiresAt, DateTime? EndedAt, string Status, IReadOnlyList<AttendanceReportRowDto> Students);
