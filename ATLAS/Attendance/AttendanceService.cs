using System.Security.Cryptography;
using System.Text;
using ATLAsDCL.Data;
using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATLAS.Attendance;

/// <summary>
/// IAttendance Service.
/// </summary>
public interface IAttendanceService
{
    Task<AttendanceSessionDto> StartAsync(int courseOfferingId, int userId, int durationMinutes, CancellationToken cancellationToken);
    Task<AttendanceQrDto?> GetCurrentQrAsync(Guid sessionId, CancellationToken cancellationToken);
    /// <summary>
    /// Records a scan after the device binding has been proved (MED-17).
    /// </summary>
    Task<AttendanceScanResult> RecordScanAsync(AttendanceScanRequest request, int studentUserId, CancellationToken cancellationToken);
    Task<AttendanceSessionDto?> EndAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<AttendanceReportDto?> GetReportAsync(Guid sessionId, CancellationToken cancellationToken);
}

/// <summary>
/// Attendance Service.
/// </summary>
public sealed class AttendanceService(AtLASDbContext context, TimeProvider timeProvider, IDeviceBindingService deviceBinding) : IAttendanceService
{
    // QR payloads are verified server-side; a captured payload is valid for one five-second slot only.
    /// <summary>
    /// Rotation Seconds.
    /// </summary>
    private const int RotationSeconds = 5;

    /// <summary>
    /// Start Async.
    /// </summary>
    public async Task<AttendanceSessionDto> StartAsync(int courseOfferingId, int userId, int durationMinutes, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!await context.CourseOfferings.AnyAsync(x => x.CourseOfferingID == courseOfferingId, cancellationToken))
            throw new AttendanceValidationException("The course offering was not found.");

        var staleSessions = await context.AttendanceSessions
            .Where(x => x.CourseOfferingId == courseOfferingId && x.Status == AttendanceSessionStatus.Active && x.ExpiresAt <= now)
            .ToListAsync(cancellationToken);

        if (staleSessions.Count > 0)
        {
            foreach (var s in staleSessions)
            {
                s.Status = AttendanceSessionStatus.Expired;
                s.EndedAt ??= s.ExpiresAt;
            }
            await context.SaveChangesAsync(cancellationToken);
        }

        var active = await context.AttendanceSessions
            .AnyAsync(x => x.CourseOfferingId == courseOfferingId && x.Status == AttendanceSessionStatus.Active && x.ExpiresAt > now, cancellationToken);
        if (active) throw new AttendanceConflictException("An attendance session is already active for this course offering.");

        var session = new AttendanceSession
        {
            CourseOfferingId = courseOfferingId,
            CreatedByUserId = userId,
            StartedAt = now,
            ExpiresAt = now.AddMinutes(durationMinutes),
            SigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        };
        context.AttendanceSessions.Add(session);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // LOW-09: the filtered unique index caught the race that the check
            // above cannot. Without this the losing instructor would receive a 500
            // for what is really a conflict they can act on - they already have a
            // session running for this offering.
            throw new AttendanceConflictException("An attendance session is already active for this course offering.");
        }

        return ToDto(session, 0);
    }

    /// <summary>
    /// Get Current Qr Async.
    /// </summary>
    public async Task<AttendanceQrDto?> GetCurrentQrAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await context.AttendanceSessions.SingleOrDefaultAsync(x => x.AttendanceSessionId == sessionId, cancellationToken);
        if (session is null) return null;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        ExpireIfNeeded(session, now);
        if (context.Entry(session).State == EntityState.Modified) await context.SaveChangesAsync(cancellationToken);
        if (session.Status != AttendanceSessionStatus.Active) return null;

        var slot = new DateTimeOffset(now).ToUnixTimeSeconds() / RotationSeconds;
        var validUntil = DateTimeOffset.FromUnixTimeSeconds((slot + 1) * RotationSeconds).UtcDateTime;
        return new AttendanceQrDto(session.AttendanceSessionId, BuildToken(session, slot), validUntil, session.ExpiresAt);
    }

    /// <summary>
    /// Record Scan Async.
    /// </summary>
    public async Task<AttendanceScanResult> RecordScanAsync(AttendanceScanRequest request, int studentUserId, CancellationToken cancellationToken)
    {
        var parsed = ParseToken(request.QrToken);
        if (parsed is null) throw new AttendanceValidationException("The QR code is malformed.");

        // MED-17: nothing about the device identifier is taken on trust. The pairing
        // token proves this server paired the device, and the per-scan signature
        // proves the device still holds the secret it was issued. Both are checked
        // before the identifier is allowed to identify anything.
        var device = await deviceBinding.VerifyScanAsync(
            request.DeviceId, request.PairingToken, request.DeviceSignature, request.QrToken, studentUserId, cancellationToken);

        var session = await context.AttendanceSessions.SingleOrDefaultAsync(x => x.AttendanceSessionId == parsed.Value.SessionId, cancellationToken);
        if (session is null) throw new AttendanceValidationException("The attendance session was not found.");
        var now = timeProvider.GetUtcNow().UtcDateTime;
        ExpireIfNeeded(session, now);
        if (context.Entry(session).State == EntityState.Modified) await context.SaveChangesAsync(cancellationToken);
        if (session.Status != AttendanceSessionStatus.Active) throw new AttendanceValidationException("This attendance session is closed.");
        var currentSlot = new DateTimeOffset(now).ToUnixTimeSeconds() / RotationSeconds;

        // LOW-09: the code only accepted the current slot, so a code displayed at
        // the very end of its five-second window was rejected while the student was
        // still looking at it. The HMAC binds the slot into the signature, so
        // accepting the previous slot cannot be forged - it only widens the window
        // by one rotation, and only for a code this server issued moments earlier.
        // Without the grace period the scan has to be retried, which is exactly what
        // students do when the app tells them "this QR code has expired" while the
        // code is still on the screen.
        var previousSlot = currentSlot - 1;
        var slotIsAcceptable = parsed.Value.Slot == currentSlot ||
                               parsed.Value.Slot == previousSlot;

        if (!slotIsAcceptable || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(parsed.Value.Signature), Encoding.UTF8.GetBytes(Sign(session, parsed.Value.Slot))))
            throw new AttendanceValidationException("This QR code has expired. Scan the currently displayed code.");

        var student = await context.Students.SingleOrDefaultAsync(x => x.UserId == studentUserId && x.IsActive, cancellationToken);
        if (student is null) throw new AttendanceValidationException("The signed-in account is not an active student.");
        var enrolled = await context.Enrollments.AnyAsync(x => x.StudentID == student.StudentId && x.CourseOfferingID == session.CourseOfferingId, cancellationToken);
        if (!enrolled) throw new AttendanceValidationException("You are not enrolled in this course offering.");

        // Now meaningful: DeviceHash comes from an identifier the server paired, so
        // a student cannot mint a fresh one per friend to bypass the rule below.
        var deviceHash = device.DeviceHash;
        if (await context.AttendanceRecords.AnyAsync(x => x.AttendanceSessionId == session.AttendanceSessionId && x.StudentId == student.StudentId, cancellationToken))
            throw new AttendanceConflictException("Attendance has already been recorded for this student.");
        if (await context.AttendanceRecords.AnyAsync(x => x.AttendanceSessionId == session.AttendanceSessionId && x.DeviceHash == deviceHash, cancellationToken))
            throw new AttendanceConflictException("This device has already recorded attendance for another student in this session.");

        context.AttendanceRecords.Add(new AttendanceRecord { AttendanceSessionId = session.AttendanceSessionId, StudentId = student.StudentId, DeviceHash = deviceHash, RecordedAt = now });
        try { await context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { throw new AttendanceConflictException("Attendance was already recorded by this student or device."); }
        return new AttendanceScanResult(session.AttendanceSessionId, now, "Attendance recorded.");
    }

    /// <summary>
    /// End Async.
    /// </summary>
    public async Task<AttendanceSessionDto?> EndAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await context.AttendanceSessions.Include(x => x.Records).SingleOrDefaultAsync(x => x.AttendanceSessionId == sessionId, cancellationToken);
        if (session is null) return null;
        if (session.Status == AttendanceSessionStatus.Active) { session.Status = AttendanceSessionStatus.Ended; session.EndedAt = timeProvider.GetUtcNow().UtcDateTime; await context.SaveChangesAsync(cancellationToken); }
        return ToDto(session, session.Records.Count);
    }

    /// <summary>
    /// Get Report Async.
    /// </summary>
    public async Task<AttendanceReportDto?> GetReportAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await context.AttendanceSessions.AsNoTracking().SingleOrDefaultAsync(x => x.AttendanceSessionId == sessionId, cancellationToken);
        if (session is null) return null;
        var rows = await context.Enrollments.AsNoTracking().Where(x => x.CourseOfferingID == session.CourseOfferingId)
            .Select(x => new { x.StudentID, x.Student.StudentCode, StudentName = x.Student.User!.FullName,
                RecordedAt = context.AttendanceRecords.Where(r => r.AttendanceSessionId == sessionId && r.StudentId == x.StudentID).Select(r => (DateTime?)r.RecordedAt).SingleOrDefault() })
            .OrderBy(x => x.StudentCode).ToListAsync(cancellationToken);
        return new AttendanceReportDto(session.AttendanceSessionId, session.CourseOfferingId, session.StartedAt, session.ExpiresAt, session.EndedAt, session.Status.ToString(),
            rows.Select(x => new AttendanceReportRowDto(x.StudentID, x.StudentCode, x.StudentName, x.RecordedAt.HasValue ? "Present" : "Absent", x.RecordedAt)).ToList());
    }

    /// <summary>
    /// To Dto.
    /// </summary>
    private static AttendanceSessionDto ToDto(AttendanceSession x, int count) => new(x.AttendanceSessionId, x.CourseOfferingId, x.StartedAt, x.ExpiresAt, x.EndedAt, x.Status.ToString(), count);
    /// <summary>
    /// Expire If Needed.
    /// </summary>
    private static void ExpireIfNeeded(AttendanceSession x, DateTime now) { if (x.Status == AttendanceSessionStatus.Active && x.ExpiresAt <= now) { x.Status = AttendanceSessionStatus.Expired; x.EndedAt ??= x.ExpiresAt; } }
    /// <summary>
    /// Build Token.
    /// </summary>
    private static string BuildToken(AttendanceSession session, long slot) => $"{session.AttendanceSessionId:N}.{slot}.{Sign(session, slot)}";
    /// <summary>
    /// Sign.
    /// </summary>
    private static string Sign(AttendanceSession session, long slot) { using var hmac = new HMACSHA256(Convert.FromBase64String(session.SigningKey)); return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{session.AttendanceSessionId:N}.{slot}"))); }
    /// <summary>
    /// Parse Token.
    /// </summary>
    private static (Guid SessionId, long Slot, string Signature)? ParseToken(string token) { var parts = token?.Split('.') ?? []; return parts.Length == 3 && Guid.TryParseExact(parts[0], "N", out var id) && long.TryParse(parts[1], out var slot) && parts[2].Length == 64 ? (id, slot, parts[2]) : null; }
}

/// <summary>
/// Attendance Validation Exception.
/// </summary>
public sealed class AttendanceValidationException(string message) : Exception(message);
/// <summary>
/// Attendance Conflict Exception.
/// </summary>
public sealed class AttendanceConflictException(string message) : Exception(message);
