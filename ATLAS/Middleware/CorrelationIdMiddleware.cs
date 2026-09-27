using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ATLAs.API.Middleware
{
    /// <summary>
    /// Correlation Id Middleware.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        /// <summary>
        /// Header Name.
        /// </summary>
        private const string HeaderName = "X-Correlation-ID";

        /// <summary>
        /// A caller-supplied correlation id ends up in logs, audit rows, security
        /// events and the response, so only a short, safe identifier is accepted.
        /// </summary>
        private static readonly Regex AllowedCorrelationId = new(
            "^[A-Za-z0-9-]{1,64}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// _next.
        /// </summary>
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invoke Async.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var incoming = context.Request.Headers[HeaderName].FirstOrDefault()?.Trim();

            // An id that does not match the allowed shape is replaced by the
            // trace id instead of being trusted.
            var correlationId =
                !string.IsNullOrEmpty(incoming) && AllowedCorrelationId.IsMatch(incoming)
                    ? incoming
                    : Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

            // One id everywhere: the correlation header, the response header and
            // TraceIdentifier (used by ProblemDetails.traceId, the audit log and
            // the security events) all carry the same value (LOW-06).
            context.TraceIdentifier = correlationId;

            context.Items[HeaderName] = correlationId;

            context.Response.Headers[HeaderName] = correlationId;

            using (Serilog.Context.LogContext.PushProperty(
                "CorrelationId",
                correlationId))
            {
                await _next(context);
            }
        }
    }
}
