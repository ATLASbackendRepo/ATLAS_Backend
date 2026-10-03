using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ATLAsBLL.Behaviors
{
    /// <summary>
    /// Logging Behavior.
    /// </summary>
    public class LoggingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// _logger.
        /// </summary>
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(
            ILogger<LoggingBehavior<TRequest, TResponse>> logger)
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

            _logger.LogInformation(
                "Handling {RequestName}",
                requestName);

            try
            {
                var response = await next();

                stopwatch.Stop();

                _logger.LogInformation(
                    "Handled {RequestName} successfully in {ElapsedMilliseconds} ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // MED-10: this used to log the exception at Error and rethrow,
                // which produced one entry here, one in UnhandledExceptionBehavior,
                // one in TransactionBehavior and one in the global handler - four
                // copies of the same stack trace for a single failure. That makes
                // the real error rate unreadable, because every occurrence is
                // counted four times.
                //
                // The exception is deliberately NOT logged here. The global
                // exception handler is the single place that records a failure,
                // and it knows the request method and path. This behavior reports
                // only timing, which the handler does not have.
                _logger.LogInformation(
                    "Failed to handle {RequestName} after {ElapsedMilliseconds} ms. " +
                    "The global exception handler records the failure.",
                    requestName,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}
