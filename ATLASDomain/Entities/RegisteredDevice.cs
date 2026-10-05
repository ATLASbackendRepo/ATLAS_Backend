namespace ATLASDomain.Entities;

/// <summary>
/// A device that the server has paired to a student account.
/// </summary>
/// <remarks>
/// MED-17: the scan endpoint used to accept whatever device identifier the client
/// chose to send, so the "one device may only record one student per session"
/// rule could be defeated by inventing a new identifier for every friend. A
/// device may only record attendance after the server has paired it and issued
/// it a secret, and every scan has to prove possession of that secret.
/// </remarks>
public class RegisteredDevice
{
    /// <summary>
    /// Registered Device Id.
    /// </summary>
    public int RegisteredDeviceId { get; set; }

    /// <summary>
    /// User Id of the student the device is paired to.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Device Id as chosen by the app. Public, never a secret on its own.
    /// </summary>
    public string DeviceId { get; set; } = null!;

    /// <summary>
    /// SHA-256 of <see cref="DeviceId"/>. The unique lookup key, so a device
    /// identifier is globally owned by exactly one account.
    /// </summary>
    public string DeviceIdHash { get; set; } = null!;

    /// <summary>
    /// Server-issued HMAC key the device signs each scan with. Returned to the
    /// client once, at pairing, over TLS.
    /// </summary>
    public string DeviceSecret { get; set; } = null!;

    /// <summary>
    /// Optional friendly name the student gave the device.
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// Paired At.
    /// </summary>
    public DateTime PairedAt { get; set; }

    /// <summary>
    /// Expires At. After this the pairing has to be repeated.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Revoked At. Set when the student, or an administrator, disowns the device.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// User the device is paired to.
    /// </summary>
    public User User { get; set; } = null!;
}
