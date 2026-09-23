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
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception occurred while handling {RequestName}.",
                    requestName);

                throw;
            }
        }
    }
}
