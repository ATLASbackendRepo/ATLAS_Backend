using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // تسجيل دخول الـ Request مع البيانات القادمة معه
            _logger.LogInformation("[START] Handling {RequestName} | Data: {@Request}", requestName, request);

            var response = await next();

            // تسجيل خروج الـ Request بنجاح
            _logger.LogInformation("[END] Handled {RequestName} successfully.", requestName);

            return response;
        }
    }
}