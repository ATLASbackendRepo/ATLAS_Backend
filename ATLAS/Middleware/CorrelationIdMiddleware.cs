using System.Diagnostics;

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
            var correlationId =
                context.Request.Headers[HeaderName].FirstOrDefault()
                ?? Activity.Current?.TraceId.ToString()
                ?? Guid.NewGuid().ToString();

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
