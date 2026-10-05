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
/// <remarks>
/// MED-17: DeviceId on its own proves nothing, because the client chooses it.
/// PairingToken is a server-signed proof that this device was registered, and
/// DeviceSignature proves the device still holds the secret issued at pairing and
/// that it signed this particular QR payload.
/// </remarks>
public sealed record AttendanceScanRequest(string QrToken, string DeviceId, string PairingToken, string DeviceSignature);
/// <summary>
/// Attendance Scan Result.
/// </summary>
public sealed record AttendanceScanResult(Guid SessionId, DateTime RecordedAt, string Message);
/// <summary>
/// Device Pairing Request.
/// </summary>
public sealed record DevicePairingRequest(string DeviceId, string? DeviceName = null);
/// <summary>
/// Device Pairing Dto.
/// </summary>
/// <remarks>
/// The only response that ever carries DeviceSecret. It is shown once, over TLS,
/// and the app must store it to sign subsequent scans.
/// </remarks>
public sealed record DevicePairingDto(string DeviceId, string PairingToken, string DeviceSecret, DateTime ExpiresAt);
/// <summary>
/// Registered Device Dto.
/// </summary>
public sealed record RegisteredDeviceDto(string DeviceId, string? DeviceName, DateTime PairedAt, DateTime ExpiresAt, DateTime? RevokedAt);
/// <summary>
/// Attendance Report Row Dto.
/// </summary>
public sealed record AttendanceReportRowDto(int StudentId, string StudentCode, string StudentName, string Status, DateTime? RecordedAt);

/// <summary>
/// Attendance Report Dto.
/// </summary>
public sealed record AttendanceReportDto(Guid SessionId, int CourseOfferingId, DateTime StartedAt, DateTime ExpiresAt, DateTime? EndedAt, string Status, IReadOnlyList<AttendanceReportRowDto> Students);
