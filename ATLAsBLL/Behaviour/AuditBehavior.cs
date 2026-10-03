using ATLAsBLL.Behaviors;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATLAsBLL.Behaviour
{
    /// <summary>
    /// Audit Behavior.
    /// </summary>
    /// <remarks>
    /// MED-05: auditing used to be driven by two guesses - a name prefix
    /// (Create/Update/Delete) and the single literal role "Admin". Both were
    /// wrong. <c>AssignCourseGradeCommand</c>, <c>ChangePasswordcommand</c>,
    /// <c>SaveRefreshTokenCommand</c> and every super-admin action matched
    /// neither rule, so the most sensitive writes in the system left no trace
    /// at all. Auditing is now driven by the explicit
    /// <see cref="IWriteCommand"/> marker, with the administrator roles as an
    /// additional rule so a privileged action can never slip through unmarked.
    /// </remarks>
    public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Roles whose every action is audited, even when the command carries no
        /// marker. SAdmin is included: it is the most privileged role and must
        /// not be able to act without leaving a record.
        /// </summary>
        private static readonly string[] AlwaysAuditedRoles = { "Admin", "SAdmin" };
        /// <summary>
        /// _current User Service.
        /// </summary>
        private readonly ICurrentUserService _currentUserService;
        /// <summary>
        /// _audit Service.
        /// </summary>
        private readonly IAuditService _auditService;
        /// <summary>
        /// _correlation Id Accessor.
        /// </summary>
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
        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Queries are never audited, not even when an administrator runs them.
            if (request is IQuery<TResponse>)
            {
                return await next();
            }
            if (!ShouldAudit(request))
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
                // A failed write is recorded too: the attempt itself is the
                // information an investigator needs.
                await _auditService.LogAsync(new AuditLogEntry
                {
                    UserId = _currentUserService.UserId,
                    Action = typeof(TRequest).Name,
                    Success = success,
                    CorrelationId = _correlationIdAccessor.CorrelationId
                }, cancellationToken);
            }
        }
        /// <summary>
        /// Decides whether a request belongs in the audit log.
        /// </summary>
        /// <param name="request">The request being handled.</param>
        /// <returns>True when the action must be recorded.</returns>
        private bool ShouldAudit(TRequest request)
        {
            // An explicitly excluded write states its own reason on the command.
            if (request is IUnauditedCommand)
                return false;
            // Every marked write is audited regardless of who issued it.
            if (request is IWriteCommand)
                return true;
            // A privileged role is always recorded, even for a command written
            // before the marker existed, so no admin action can be unaudited.
            var role = _currentUserService.Role?.ToString();
            return role is not null &&
                   AlwaysAuditedRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }
    }
}