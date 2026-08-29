using ATLAsBLL.Behaviors;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Behaviour
{



    public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IRequest<TResponse>
    {
        private static readonly string[] AuditedPrefixes = { "Create", "Update", "Delete" };
        private const string AdminRole = "Admin"; // لو الـ Role اسمه مختلف عندك، غيّرها هنا

        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;
        private readonly ICorrelationIdAccessor _correlationIdAccessor;

        public AuditBehavior(
            ICurrentUserService currentUserService,
            IAuditService auditService,
            ICorrelationIdAccessor correlationIdAccessor)
        {
            _currentUserService = currentUserService;
            _auditService = auditService;
            _correlationIdAccessor = correlationIdAccessor;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // الـ Queries مستبعدة تمامًا حتى لو الـ Admin هو اللي بيعمل الاستعلام
            if (request is not ICommand<TResponse>)
            {
                return await next();
            }

            var requestName = typeof(TRequest).Name;
            var isAdmin = string.Equals(_currentUserService.Role?.ToString(), AdminRole, StringComparison.OrdinalIgnoreCase);
            var matchesPrefix = AuditedPrefixes.Any(prefix => requestName.StartsWith(prefix));

            // نسجل لو: العملية من النوع اللي بنتابعه عادي، أو منفذها Admin (أيًا كان اسم الـ Command)
            var shouldAudit = matchesPrefix || isAdmin;

            if (!shouldAudit)
            {
                return await next();
            }

            var success = true;

            try
            {
                return await next();
            }
            catch
            {
                success = false;
                throw;
            }
            finally
            {
                await _auditService.LogAsync(new AuditLogEntry
                {
                    UserId = _currentUserService.UserId,
                    Action = requestName,
                    Success = success,
                    CorrelationId = _correlationIdAccessor.CorrelationId
                }, cancellationToken);
            }
        }
    }
}
