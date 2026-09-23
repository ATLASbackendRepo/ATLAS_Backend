namespace ATLAS.Claims;

/// <summary>
/// Names of the custom (non-standard) JWT claims issued by ATLAS, kept in one
/// place so token creation and claim consumers cannot drift apart.
/// </summary>
public static class AtlasClaims
{
    /// <summary>
    /// Academic identifier of the authenticated user, as typed at login.
    /// </summary>
    public const string AcademicId = "AcademicId";

    /// <summary>
    /// Primary key (<c>Students.StudentId</c>) of the student linked to the
    /// authenticated user. Only present for accounts that have a student profile.
    /// </summary>
    public const string StudentId = "StudentId";

    /// <summary>
    /// Marks a short-lived access token that may only be used to call the
    /// mandatory password-change endpoint. Issued by login when
    /// <c>Users.MustChangePassword</c> is set.
    /// </summary>
    public const string PasswordChangeOnly = "pwd_change_only";
}
