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
    int? InstitutionId { get; }
    bool IsSAdmin { get; }
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

    public int? InstitutionId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?
                .FindFirst("institution_id")?.Value;

            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public bool IsSAdmin => Role == enUserRole.SAdmin;
}
