namespace ATLAS;

/// <summary>
/// Jwt Options.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Section Name.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Key.
    /// </summary>
    public string Key { get; init; } = string.Empty;
    /// <summary>
    /// Issuer.
    /// </summary>
    public string Issuer { get; init; } = string.Empty;
    /// <summary>
    /// Audience.
    /// </summary>
    public string Audience { get; init; } = string.Empty;
    /// <summary>
    /// Expiry Minutes.
    /// </summary>
    public int ExpiryMinutes { get; init; }
}
