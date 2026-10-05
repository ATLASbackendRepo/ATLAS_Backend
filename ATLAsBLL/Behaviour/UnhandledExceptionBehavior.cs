using MediatR;
using Microsoft.Extensions.Logging;

namespace ATLAsBLL.Behaviors
{
    /// <summary>
    /// Unhandled Exception Behavior.
    /// </summary>
    public class UnhandledExceptionBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// _logger.
        /// </summary>
        private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

        public UnhandledExceptionBehavior(
            ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
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

            try
            {
                return await next();
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Request {RequestName} was cancelled.",
                    requestName);

                throw;
            }
            catch (Exception)
            {
                // MED-10: this used to log the exception at Error and rethrow. It
                // ran inside the same pipeline as LoggingBehavior and the global
                // handler, so a single failure was written four times and the
                // Error count in Seq was four times the real number of faults.
                //
                // Cancelling an in-flight command is not a fault and is still worth
                // a line, so that case is kept. Everything else is left to the
                // global exception handler, which is the single recorder and the
                // only one that also knows the request method and path.
                _logger.LogInformation(
                    "Request {RequestName} failed. The global exception handler records the failure.",
                    requestName);

                throw;
            }
        }
    }
}
