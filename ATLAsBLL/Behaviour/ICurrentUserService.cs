using ATLASDomain.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

/// <summary>
/// ICurrent User Service.
/// </summary>
public interface ICurrentUserService
{
    int? UserId { get; }
    enUserRole? Role { get; }
}
/// <summary>
/// Current User Service.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    /// <summary>
    /// _http Context Accessor.
    /// </summary>
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public enUserRole? Role
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Role)?.Value;

            return Enum.TryParse<enUserRole>(claim, out var role) ? role : null;
        }
    }
}
