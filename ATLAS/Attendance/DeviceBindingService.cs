using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ATLAsDCL.Data;
using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ATLAS.Attendance;

/// <summary>
/// The outcome of a successful device binding check.
/// </summary>
/// <param name="RegisteredDeviceId">Server-side identifier of the paired device.</param>
/// <param name="DeviceId">The device identifier exactly as the client sent it.</param>
/// <param name="DeviceHash">SHA-256 of the device identifier, stored on the attendance row.</param>
public sealed record VerifiedDevice(int RegisteredDeviceId, string DeviceId, string DeviceHash);

/// <summary>
/// IDevice Binding Service.
/// </summary>
public interface IDeviceBindingService
{
    /// <summary>
    /// Pairs a device to a student account, issuing its secret and a pairing token.
    /// </summary>
    Task<DevicePairingDto> PairAsync(int userId, string deviceId, string? deviceName, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the caller's paired devices. Secrets are never returned.
    /// </summary>
    Task<IReadOnlyList<RegisteredDeviceDto>> ListAsync(int userId, CancellationToken cancellationToken);

    /// <summary>
    /// Revokes one of the caller's paired devices.
    /// </summary>
    /// <returns>False when the device is not paired to this account.</returns>
    Task<bool> RevokeAsync(int userId, string deviceId, CancellationToken cancellationToken);

    /// <summary>
    /// Verifies that a scan comes from a paired, live device owned by the scanning
    /// student and that the device signed this exact QR payload.
    /// </summary>
    Task<VerifiedDevice> VerifyScanAsync(string deviceId, string pairingToken, string deviceSignature, string qrToken, int studentUserId, CancellationToken cancellationToken);
}

/// <summary>
/// Device Binding Service.
/// </summary>
/// <remarks>
/// MED-17: two independent proofs are required before a scan is trusted. The
/// pairing token is signed with a key the client never sees, so only this server
/// can mint one; it binds the device identifier to an account and an expiry. The
/// per-scan signature is made with the device secret issued at pairing and covers
/// the QR payload, so a captured request body cannot be replayed against a
/// different code. Neither is sufficient alone: the token proves the device was
/// registered, the signature proves the registrant still holds it.
/// </remarks>
public sealed class DeviceBindingService(
    AtLASDbContext context,
    TimeProvider timeProvider,
    IOptions<DeviceBindingOptions> deviceBindingOptions,
    IOptions<JwtOptions> jwtOptions) : IDeviceBindingService
{
    /// <summary>
    /// Device Id Pattern.
    /// </summary>
    private static readonly Regex DeviceIdPattern = new("^[A-Za-z0-9_-]{16,64}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Device Not Paired Message.
    /// </summary>
    private const string DeviceNotPairedMessage = "This device is not registered for attendance scanning. Pair the device first.";

    /// <summary>
    /// Pairing Key.
    /// </summary>
    private readonly byte[] _pairingKey = ResolvePairingKey(deviceBindingOptions.Value.Key, jwtOptions.Value.Key);

    /// <summary>
    /// Pair Async.
    /// </summary>
    public async Task<DevicePairingDto> PairAsync(int userId, string deviceId, string? deviceName, CancellationToken cancellationToken)
    {
        if (!IsValidDeviceId(deviceId))
            throw new AttendanceValidationException("A device identifier of 16 to 64 characters using A-Z, a-z, 0-9, '-' and '_' is required.");
        if (deviceName is { Length: > 100 })
            throw new AttendanceValidationException("The device name must be 100 characters or fewer.");

        var options = deviceBindingOptions.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var hash = HashDeviceId(deviceId);
        var existing = await context.RegisteredDevices.SingleOrDefaultAsync(x => x.DeviceIdHash == hash, cancellationToken);

        if (existing is not null && existing.UserId != userId)
            throw new AttendanceConflictException("This device is already registered to another account.");

        if (existing is null)
        {
            var activeCount = await context.RegisteredDevices.CountAsync(
                x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now, cancellationToken);
            if (activeCount >= options.MaxDevicesPerStudent)
                throw new AttendanceConflictException($"A student may have at most {options.MaxDevicesPerStudent} devices registered for attendance scanning.");

            existing = new RegisteredDevice
            {
                UserId = userId,
                DeviceId = deviceId,
                DeviceIdHash = hash,
                DeviceSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                DeviceName = deviceName,
                PairedAt = now,
                ExpiresAt = now.AddDays(options.PairingTokenLifetimeDays)
            };
            context.RegisteredDevices.Add(existing);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Two pairing requests for the same identifier can arrive together;
                // the unique index on DeviceIdHash catches the race this check
                // cannot, and the loser gets a conflict rather than a 500.
                throw new AttendanceConflictException("This device is already registered.");
            }
        }
        else if (existing.RevokedAt is not null || existing.ExpiresAt <= now)
        {
            // Re-pairing an expired or revoked device mints a fresh secret, so the
            // old one stops working as soon as this registration is replaced.
            existing.RevokedAt = null;
            existing.PairedAt = now;
            existing.ExpiresAt = now.AddDays(options.PairingTokenLifetimeDays);
            existing.DeviceSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            if (deviceName is not null) existing.DeviceName = deviceName;
            await context.SaveChangesAsync(cancellationToken);
        }

        return new DevicePairingDto(
            existing.DeviceId,
            CreatePairingToken(existing.UserId, existing.DeviceId, existing.ExpiresAt),
            existing.DeviceSecret,
            existing.ExpiresAt);
    }

    /// <summary>
    /// List Async.
    /// </summary>
    public async Task<IReadOnlyList<RegisteredDeviceDto>> ListAsync(int userId, CancellationToken cancellationToken)
    {
        var devices = await context.RegisteredDevices.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.PairedAt)
            .ToListAsync(cancellationToken);

        return devices
            .Select(x => new RegisteredDeviceDto(x.DeviceId, x.DeviceName, x.PairedAt, x.ExpiresAt, x.RevokedAt))
            .ToList();
    }

    /// <summary>
    /// Revoke Async.
    /// </summary>
    public async Task<bool> RevokeAsync(int userId, string deviceId, CancellationToken cancellationToken)
    {
        if (!IsValidDeviceId(deviceId)) return false;

        var hash = HashDeviceId(deviceId);
        var device = await context.RegisteredDevices.SingleOrDefaultAsync(
            x => x.DeviceIdHash == hash && x.UserId == userId, cancellationToken);
        if (device is null || device.RevokedAt is not null) return false;

        device.RevokedAt = timeProvider.GetUtcNow().UtcDateTime;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }


    /// <summary>
    /// Verify Scan Async.
    /// </summary>
    public async Task<VerifiedDevice> VerifyScanAsync(string deviceId, string pairingToken, string deviceSignature, string qrToken, int studentUserId, CancellationToken cancellationToken)
    {
        if (!IsValidDeviceId(deviceId))
            throw new AttendanceValidationException("A valid app-generated device identifier is required.");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var hash = HashDeviceId(deviceId);

        var parsed = ParsePairingToken(pairingToken);
        if (parsed is null) throw new AttendanceValidationException(DeviceNotPairedMessage);

        // The pairing token is signed with a server-only key, so a fabricated device
        // identifier cannot carry one. Comparing in constant time keeps the token
        // from turning into an oracle for that key material.
        var payload = $"{parsed.Value.DeviceId}.{parsed.Value.UserId}.{parsed.Value.ExpiresUnix}";
        if (!FixedTimeEquals(SignPayload(payload), parsed.Value.Signature))
            throw new AttendanceValidationException(DeviceNotPairedMessage);

        if (parsed.Value.ExpiresUnix <= new DateTimeOffset(now, TimeSpan.Zero).ToUnixTimeSeconds())
            throw new AttendanceValidationException("This device pairing has expired. Pair the device again.");

        if (!string.Equals(parsed.Value.DeviceId, deviceId, StringComparison.Ordinal) || parsed.Value.UserId != studentUserId)
            throw new AttendanceValidationException(DeviceNotPairedMessage);

        var device = await context.RegisteredDevices.SingleOrDefaultAsync(x => x.DeviceIdHash == hash, cancellationToken);
        if (device is null || device.RevokedAt is not null)
            throw new AttendanceValidationException(DeviceNotPairedMessage);
        if (device.ExpiresAt <= now)
            throw new AttendanceValidationException("This device pairing has expired. Pair the device again.");
        if (device.UserId != studentUserId)
            throw new AttendanceValidationException("This device is paired to a different account.");

        // The signature covers the QR payload itself, so a body captured from one
        // scan cannot be replayed against a different code, a different session, or
        // another student's attempt at the same code.
        var expectedSignature = SignWithSecret(device.DeviceSecret, deviceId + qrToken);
        if (!FixedTimeEquals(expectedSignature, deviceSignature))
            throw new AttendanceValidationException("The device signature for this scan could not be verified.");

        return new VerifiedDevice(device.RegisteredDeviceId, device.DeviceId, hash);
    }


    /// <summary>
    /// Is Valid Device Id.
    /// </summary>
    private static bool IsValidDeviceId(string? deviceId) => deviceId is not null && DeviceIdPattern.IsMatch(deviceId);

    /// <summary>
    /// Hash Device Id.
    /// </summary>
    private static string HashDeviceId(string deviceId) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(deviceId)));

    /// <summary>
    /// Create Pairing Token.
    /// </summary>
    private string CreatePairingToken(int userId, string deviceId, DateTime expiresAtUtc)
    {
        var payload = $"{deviceId}.{userId}.{new DateTimeOffset(expiresAtUtc, TimeSpan.Zero).ToUnixTimeSeconds()}";
        return $"{payload}.{SignPayload(payload)}";
    }

    /// <summary>
    /// Sign Payload.
    /// </summary>
    private string SignPayload(string payload)
    {
        using var hmac = new HMACSHA256(_pairingKey);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    }

    /// <summary>
    /// Sign With Secret.
    /// </summary>
    private static string SignWithSecret(string deviceSecret, string message)
    {
        using var hmac = new HMACSHA256(Convert.FromBase64String(deviceSecret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(message)));
    }


    /// <summary>
    /// Parse Pairing Token.
    /// </summary>
    private static (string DeviceId, int UserId, long ExpiresUnix, string Signature)? ParsePairingToken(string? token)
    {
        var parts = token?.Split('.') ?? [];
        return parts.Length == 4 &&
               parts[3].Length == 64 &&
               int.TryParse(parts[1], out var userId) &&
               long.TryParse(parts[2], out var expires)
            ? (parts[0], userId, expires, parts[3])
            : null;
    }

    /// <summary>
    /// Fixed Time Equals.
    /// </summary>
    private static bool FixedTimeEquals(string expected, string? actual) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(actual ?? string.Empty));

    /// <summary>
    /// Resolve Pairing Key.
    /// </summary>
    private static byte[] ResolvePairingKey(string configuredKey, string jwtKey)
    {
        if (!string.IsNullOrWhiteSpace(configuredKey))
        {
            if (configuredKey.Length < 64)
                throw new InvalidOperationException("Attendance:DeviceBinding:Key must be at least 64 characters long.");
            return SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));
        }

        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException(
                "Device binding needs signing key material. Set Attendance:DeviceBinding:Key, or supply Jwt:Key so it can be derived.");

        // Domain separation: the same value is never used as HMAC key material for
        // two different purposes, so a pairing token can never be confused with
        // anything else signed with the JWT key.
        return HMACSHA256.HashData(Encoding.UTF8.GetBytes(jwtKey), Encoding.UTF8.GetBytes("ATLAS:Attendance:DeviceBinding:v1"));
    }
}

