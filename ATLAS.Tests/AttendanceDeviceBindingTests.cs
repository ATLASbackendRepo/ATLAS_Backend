using System.Security.Cryptography;
using System.Text;
using ATLAsDCL.Data;
using ATLAS.Attendance;
using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace ATLAS.Tests;

/// <summary>
/// MED-17: attendance used to trust whatever device identifier the client sent, so
/// the "one device per student per session" rule fell over as soon as someone
/// invented a fresh identifier. These tests pin the pairing and signature checks
/// that replace it.
/// </summary>
public class AttendanceDeviceBindingTests
{
    /// <summary>
    /// Key used to sign pairing tokens in these tests.
    /// </summary>
    private const string TestSigningKey = "unit-test-device-binding-key-that-is-long-enough-for-the-sixty-four-character-minimum-0123456789";

    /// <summary>
    /// QR codes rotate every five seconds.
    /// </summary>
    private const int RotationSeconds = 5;

    /// <summary>
    /// The two student accounts in the fixture.
    /// </summary>
    private const int StudentA = 11;
    private const int StudentB = 22;

    /// <summary>
    /// The course offering both students are enrolled in.
    /// </summary>
    private const int OfferingId = 7;

    /// <summary>
    /// A clock the tests can move, so expiry can be reached without waiting.
    /// </summary>
    private sealed class FixedClock(DateTimeOffset utcNow) : TimeProvider
    {
        /// <summary>
        /// Utc Now.
        /// </summary>
        public DateTimeOffset UtcNow { get; set; } = utcNow;

        /// <summary>
        /// Advance.
        /// </summary>
        public void Advance(TimeSpan by) => UtcNow += by;

        /// <inheritdoc />
        public override DateTimeOffset GetUtcNow() => UtcNow;
    }

    /// <summary>
    /// Seeds a course offering, an active attendance session and two enrolled
    /// students, and builds the services against that data.
    /// </summary>
    private sealed class Scene : IDisposable
    {
        /// <summary>
        /// Context.
        /// </summary>
        public AtLASDbContext Context { get; } = TestDbContextFactory.Create();

        /// <summary>
        /// Clock.
        /// </summary>
        public FixedClock Clock { get; } = new(new DateTimeOffset(2026, 10, 4, 10, 0, 0, TimeSpan.Zero));

        /// <summary>
        /// Session.
        /// </summary>
        public AttendanceSession Session { get; }

        /// <summary>
        /// Creates the fixture.
        /// </summary>
        public Scene()
        {
            var now = Clock.GetUtcNow().UtcDateTime;

            Context.Users.AddRange(
                new User { UserId = StudentA, FullName = "Student A", Password = "x", IsActive = true, AcademicId = "STU-A" },
                new User { UserId = StudentB, FullName = "Student B", Password = "x", IsActive = true, AcademicId = "STU-B" });
            Context.Semesters.Add(new Semester { SemesterID = 1, SemesterName = "Spring", AcademicYear = "2026" });
            Context.Courses.Add(new Course { CourseID = 1, C_Name = "Math", Credit_Hours = 3 });
            Context.CourseOfferings.Add(new CourseOffering { CourseOfferingID = OfferingId, CourseID = 1, SemesterID = 1 });
            Context.Students.AddRange(
                new Student { StudentId = 1, UserId = StudentA, StudentCode = "A1", AcademicId = "A1", IsActive = true },
                new Student { StudentId = 2, UserId = StudentB, StudentCode = "B1", AcademicId = "B1", IsActive = true });
            Context.Enrollments.AddRange(
                new Enrollment { EnrollmentID = 1, StudentID = 1, CourseOfferingID = OfferingId },
                new Enrollment { EnrollmentID = 2, StudentID = 2, CourseOfferingID = OfferingId });

            Session = new AttendanceSession
            {
                CourseOfferingId = OfferingId,
                CreatedByUserId = StudentA,
                StartedAt = now,
                ExpiresAt = now.AddDays(7),
                SigningKey = Convert.ToBase64String(new byte[32])
            };
            Context.AttendanceSessions.Add(Session);
            Context.SaveChanges();
        }

        /// <summary>
        /// Builds the device binding service over this fixture.
        /// </summary>
        public DeviceBindingService Devices(int maxDevices = 3, int pairingLifetimeDays = 180) =>
            new(Context, Clock,
                Options.Create(new DeviceBindingOptions
                {
                    Key = TestSigningKey,
                    MaxDevicesPerStudent = maxDevices,
                    PairingTokenLifetimeDays = pairingLifetimeDays
                }),
                Options.Create(new JwtOptions()));

        /// <summary>
        /// Builds the attendance service over this fixture.
        /// </summary>
        public AttendanceService Attendance() => new(Context, Clock, Devices());

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose() => Context.Dispose();
    }

    /// <summary>
    /// A device identifier in the format the API accepts.
    /// </summary>
    private const string DeviceId = "device-0000000000000001";

    /// <summary>
    /// A second device identifier.
    /// </summary>
    private const string OtherDeviceId = "device-0000000000000002";

    /// <summary>
    /// Builds a QR payload the way GetCurrentQrAsync does, so the scan sees a code
    /// this server would really have issued for the current rotation slot.
    /// </summary>
    private static string BuildQrToken(AttendanceSession session, TimeProvider clock)
    {
        var slot = new DateTimeOffset(clock.GetUtcNow().UtcDateTime).ToUnixTimeSeconds() / RotationSeconds;
        using var hmac = new HMACSHA256(Convert.FromBase64String(session.SigningKey));
        var signature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{session.AttendanceSessionId:N}.{slot}")));
        return $"{session.AttendanceSessionId:N}.{slot}.{signature}";
    }

    /// <summary>
    /// Signs a scan the way a paired app does.
    /// </summary>
    private static string SignScan(string deviceSecret, string deviceId, string qrToken)
    {
        using var hmac = new HMACSHA256(Convert.FromBase64String(deviceSecret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(deviceId + qrToken)));
    }

    /// <summary>
    /// Pairs a device and builds a scan request that is correctly signed for it.
    /// </summary>
    private static async Task<AttendanceScanRequest> PairAndSignAsync(Scene scene, int userId, string deviceId, DeviceBindingService? devices = null)
    {
        devices ??= scene.Devices();
        var pairing = await devices.PairAsync(userId, deviceId, null, CancellationToken.None);
        var qr = BuildQrToken(scene.Session, scene.Clock);
        return new AttendanceScanRequest(qr, deviceId, pairing.PairingToken, SignScan(pairing.DeviceSecret, deviceId, qr));
    }

    /// <summary>
    /// A paired device with a correct signature records attendance.
    /// </summary>
    [Fact]
    public async Task Scan_PairedDeviceWithValidSignature_RecordsAttendance()
    {
        using var scene = new Scene();
        var request = await PairAndSignAsync(scene, StudentA, DeviceId);

        var result = await scene.Attendance().RecordScanAsync(request, StudentA, CancellationToken.None);

        Assert.Equal(scene.Session.AttendanceSessionId, result.SessionId);
        Assert.Single(await scene.Context.AttendanceRecords.ToListAsync());
    }

    /// <summary>
    /// The MED-17 defect: an identifier the server never paired carries no proof, so
    /// it cannot record anything no matter how well formed it looks.
    /// </summary>
    [Fact]
    public async Task Scan_FabricatedDeviceIdentifier_IsRejected()
    {
        using var scene = new Scene();
        var qr = BuildQrToken(scene.Session, scene.Clock);
        var request = new AttendanceScanRequest(qr, DeviceId, string.Empty, string.Empty);

        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => scene.Attendance().RecordScanAsync(request, StudentA, CancellationToken.None));

        Assert.Contains("not registered", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await scene.Context.AttendanceRecords.ToListAsync());
    }

    /// <summary>
    /// A pairing token alone is not enough: the device must also prove it still
    /// holds the secret it was issued.
    /// </summary>
    [Fact]
    public async Task Scan_WithoutDeviceSignature_IsRejected()
    {
        using var scene = new Scene();
        var devices = scene.Devices();
        var pairing = await devices.PairAsync(StudentA, DeviceId, null, CancellationToken.None);
        var qr = BuildQrToken(scene.Session, scene.Clock);
        var request = new AttendanceScanRequest(qr, DeviceId, pairing.PairingToken, string.Empty);

        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => scene.Attendance().RecordScanAsync(request, StudentA, CancellationToken.None));

        Assert.Contains("signature", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await scene.Context.AttendanceRecords.ToListAsync());
    }

    /// <summary>
    /// The signature covers the QR payload, so a body captured from one code cannot
    /// be replayed against another.
    /// </summary>
    [Fact]
    public async Task Scan_SignatureTakenFromADifferentQrPayload_IsRejected()
    {
        using var scene = new Scene();
        var devices = scene.Devices();
        var pairing = await devices.PairAsync(StudentA, DeviceId, null, CancellationToken.None);

        var qr = BuildQrToken(scene.Session, scene.Clock);
        var stolen = SignScan(pairing.DeviceSecret, DeviceId, qr);

        var otherQr = qr[..^1] + (qr[^1] == '0' ? '1' : '0');
        var request = new AttendanceScanRequest(otherQr, DeviceId, pairing.PairingToken, stolen);

        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => scene.Attendance().RecordScanAsync(request, StudentA, CancellationToken.None));

        Assert.Contains("signature", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await scene.Context.AttendanceRecords.ToListAsync());
    }

    /// <summary>
    /// A pairing token issued for one device does not vouch for another.
    /// </summary>
    [Fact]
    public async Task Scan_PairingTokenIssuedForAnotherDevice_IsRejected()
    {
        using var scene = new Scene();
        var devices = scene.Devices();
        var mine = await devices.PairAsync(StudentA, DeviceId, null, CancellationToken.None);
        var theirs = await devices.PairAsync(StudentA, OtherDeviceId, null, CancellationToken.None);

        var qr = BuildQrToken(scene.Session, scene.Clock);
        var request = new AttendanceScanRequest(qr, DeviceId, theirs.PairingToken, SignScan(mine.DeviceSecret, DeviceId, qr));

        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => scene.Attendance().RecordScanAsync(request, StudentA, CancellationToken.None));

        Assert.Contains("not registered", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// A device stops working the moment the student disowns it, even though the
    /// pairing token it holds has not reached its expiry yet.
    /// </summary>
    [Fact]
    public async Task Scan_AfterTheDeviceIsRevoked_IsRejected()
    {
        using var scene = new Scene();
        var devices = scene.Devices();
        var request = await PairAndSignAsync(scene, StudentA, DeviceId, devices);

        Assert.True(await devices.RevokeAsync(StudentA, DeviceId, CancellationToken.None));

        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => scene.Attendance().RecordScanAsync(request, StudentA, CancellationToken.None));

        Assert.Contains("not registered", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await scene.Context.AttendanceRecords.ToListAsync());
    }

    /// <summary>
    /// A pairing token stops being accepted once its lifetime is over.
    /// </summary>
    [Fact]
    public async Task Scan_AfterThePairingTokenExpires_IsRejected()
    {
        using var scene = new Scene();
        var devices = scene.Devices(pairingLifetimeDays: 1);
        var pairing = await devices.PairAsync(StudentA, DeviceId, null, CancellationToken.None);

        // Past the one-day lifetime, but with a freshly signed QR for the new slot,
        // so the pairing is the only thing that has lapsed.
        scene.Clock.Advance(TimeSpan.FromHours(25));
        var qr = BuildQrToken(scene.Session, scene.Clock);
        var expired = new AttendanceScanRequest(qr, DeviceId, pairing.PairingToken, SignScan(pairing.DeviceSecret, DeviceId, qr));

        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => scene.Attendance().RecordScanAsync(expired, StudentA, CancellationToken.None));

        Assert.Contains("expired", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await scene.Context.AttendanceRecords.ToListAsync());
    }

    /// <summary>
    /// A device identifier belongs to one account. Claiming someone else's is a
    /// conflict, not something the second account can quietly take over.
    /// </summary>
    [Fact]
    public async Task Pair_DeviceAlreadyPairedToAnotherAccount_IsAConflict()
    {
        using var scene = new Scene();
        var devices = scene.Devices();
        await devices.PairAsync(StudentB, DeviceId, null, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<AttendanceConflictException>(
            () => devices.PairAsync(StudentA, DeviceId, null, CancellationToken.None));

        Assert.Contains("another account", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Even with a token that verifies, the registration row is what decides whose
    /// device this is. If it has been reassigned the scan is refused.
    /// </summary>
    [Fact]
    public async Task Scan_WhenTheRegistrationRowBelongsToAnotherAccount_IsRejected()
    {
        using var scene = new Scene();
        var devices = scene.Devices();
        var pairing = await devices.PairAsync(StudentA, DeviceId, null, CancellationToken.None);

        var row = scene.Context.RegisteredDevices.Single(x => x.DeviceId == DeviceId);
        row.UserId = StudentB;
        await scene.Context.SaveChangesAsync();

        var qr = BuildQrToken(scene.Session, scene.Clock);
        var request = new AttendanceScanRequest(qr, DeviceId, pairing.PairingToken, SignScan(pairing.DeviceSecret, DeviceId, qr));

        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => scene.Attendance().RecordScanAsync(request, StudentA, CancellationToken.None));

        Assert.Contains("different account", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// One student cannot register an unbounded number of devices, because each one
    /// would otherwise be able to record attendance separately.
    /// </summary>
    [Fact]
    public async Task Pair_BeyondTheConfiguredDeviceCap_IsRejected()
    {
        using var scene = new Scene();
        var devices = scene.Devices(maxDevices: 2);

        await devices.PairAsync(StudentA, DeviceId, null, CancellationToken.None);
        await devices.PairAsync(StudentA, OtherDeviceId, null, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<AttendanceConflictException>(
            () => devices.PairAsync(StudentA, "device-0000000000000003", null, CancellationToken.None));

        Assert.Contains("at most 2", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Pairing rejects identifiers the token format cannot carry unambiguously.
    /// </summary>
    [Fact]
    public async Task Pair_MalformedDeviceIdentifier_IsRejected()
    {
        using var scene = new Scene();
        var devices = scene.Devices();

        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => devices.PairAsync(StudentA, "too short", null, CancellationToken.None));

        Assert.Contains("device identifier", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The pairing token is signed with a key the client never receives, so a token
    /// the client builds itself - correct shape, correct claims, forged signature -
    /// is worth nothing.
    /// </summary>
    [Fact]
    public async Task Scan_PairingTokenForgedByTheClient_IsRejected()
    {
        using var scene = new Scene();

        var expires = scene.Clock.GetUtcNow().AddDays(30).ToUnixTimeSeconds();
        var forgedToken = $"{DeviceId}.{StudentA}.{expires}." + new string('0', 64);
        var forgedSignature = new string('0', 64);

        var qr = BuildQrToken(scene.Session, scene.Clock);
        var request = new AttendanceScanRequest(qr, DeviceId, forgedToken, forgedSignature);

        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => scene.Attendance().RecordScanAsync(request, StudentA, CancellationToken.None));

        Assert.Contains("not registered", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await scene.Context.AttendanceRecords.ToListAsync());
    }
}
