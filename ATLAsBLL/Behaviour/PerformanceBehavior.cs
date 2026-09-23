using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ATLAsBLL.Behaviors
{
    /// <summary>
    /// Performance Behavior.
    /// </summary>
    public class PerformanceBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// _logger.
        /// </summary>
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

        public PerformanceBehavior(
            ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();

                stopwatch.Stop();

                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                if (elapsedMilliseconds >= 2000)
                {
                    _logger.LogError(
                        "Critical slow request detected. Request: {RequestName}, Duration: {ElapsedMilliseconds} ms",
                        requestName,
                        elapsedMilliseconds);
                }
                else if (elapsedMilliseconds >= 500)
                {
                    _logger.LogWarning(
                        "Slow request detected. Request: {RequestName}, Duration: {ElapsedMilliseconds} ms",
                        requestName,
                        elapsedMilliseconds);
                }
                else
                {
                    _logger.LogDebug(
                        "Request {RequestName} completed in {ElapsedMilliseconds} ms",
                        requestName,
                        elapsedMilliseconds);
                }

                return response;
            }
            catch
            {
                stopwatch.Stop();
                throw;
            }
        }
    }
}
