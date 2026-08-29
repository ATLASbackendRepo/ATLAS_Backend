using ATLASDomain.InterFaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Auth
{
    public class SecurityAuditAuthorizationHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        // مفيش ISecurityEventService هنا خالص في الـ Constructor

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (!authorizeResult.Succeeded)
            {
                var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;

                // بنجيب الـ Scoped Service من الـ Request نفسه وقت التنفيذ
                var securityEventService = context.RequestServices.GetRequiredService<ISecurityEventService>();

                await securityEventService.LogAsync(new SecurityEventEntry
                {
                    EventType = isAuthenticated ? "UnauthorizedAccess" : "UnauthenticatedAccess",
                    UserId = TryGetUserId(context),
                    Severity = isAuthenticated ? "Medium" : "Low",
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    Description = $"{context.Request.Method} {context.Request.Path}",
                    CorrelationId = context.TraceIdentifier
                });
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        private static int? TryGetUserId(HttpContext context)
        {
            var claim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    
}
}
