using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Behaviors
{
    // الواجهة دي بنعملها عشان نعلم بيها الـ Commands اللي محتاجة تسجيل مين اللي نفذها
    public interface IAuditableCommand
    {
        int ExecutingUserId { get; set; }
    }

    public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditBehavior(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is IAuditableCommand auditableCommand)
            {
                // سحب الـ User ID الموثق من الـ Token وحقنه في الـ Command أوتوماتيكياً
                if (_currentUserService.UserId.HasValue)
                {
                    auditableCommand.ExecutingUserId = _currentUserService.UserId.Value;
                }
            }

            return await next();
        }
    }
}