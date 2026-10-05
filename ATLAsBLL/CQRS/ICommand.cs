using MediatR;

namespace ATLAsBLL.CQRS;
/// <summary>
/// ICommand.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
/// <summary>
/// Marks a request that changes state (MED-05).
/// </summary>
/// <remarks>
/// Several write requests used to be declared as a bare <c>IRequest</c>, which
/// silently skipped both the transaction opened by
/// <c>TransactionBehavior</c> and the audit pipeline: a failed enrollment,
/// profile update or password change could leave a half-applied write, and the
/// action left no trace. This is a plain, non-generic marker so that it
/// composes with commands returning a value (<c>ICommand&lt;bool&gt;</c>,
/// <c>ICommand&lt;int&gt;</c>) as well as with unit-returning ones.
/// </remarks>
public interface IWriteCommand
{
}
/// <summary>
/// Marks a write that is deliberately excluded from the audit log and records
/// why (MED-05).
/// </summary>
/// <remarks>
/// Reserved for high-frequency internal writes such as refresh-token rotation,
/// where one audit row per call would be pure volume. The reason is declared on
/// the command rather than being silently dropped.
/// </remarks>
public interface IUnauditedCommand
{
    /// <summary>
    /// Why this write is excluded from the audit log.
    /// </summary>
    string UnauditedReason { get; }
}
