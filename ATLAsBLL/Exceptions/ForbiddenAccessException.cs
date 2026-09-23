namespace ATLAsBLL.Exceptions;

/// <summary>
/// Thrown when a user attempts an action they are not authorized to perform.
/// Controllers catch this and return a 403 Forbidden response.
/// </summary>
public class ForbiddenAccessException : Exception
{
    /// <summary>
    /// base.
    /// </summary>
    public ForbiddenAccessException(string message) : base(message) { }
}
