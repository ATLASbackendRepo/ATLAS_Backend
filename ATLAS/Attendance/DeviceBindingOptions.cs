namespace ATLAS.Attendance;

/// <summary>
/// Device Binding Options.
/// </summary>
public sealed class DeviceBindingOptions
{
    /// <summary>
    /// Section Name.
    /// </summary>
    public const string SectionName = "Attendance:DeviceBinding";

    /// <summary>
    /// Optional explicit key used to sign device pairing tokens. When it is left
    /// empty the key is derived from <c>Jwt:Key</c> with a domain-separation label,
    /// so a deployment does not have to provision a second secret to enable MED-17.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// How long a pairing token stays valid, in days.
    /// </summary>
    public int PairingTokenLifetimeDays { get; init; } = 180;

    /// <summary>
    /// How many active devices one student may have registered at a time.
    /// </summary>
    public int MaxDevicesPerStudent { get; init; } = 3;
}
