namespace ATLAsBLL.CQRS;

/// <summary>
/// Marks a command that opens and commits its own database transaction.
/// </summary>
/// <remarks>
/// <see cref="ATLAsBLL.Behaviors.TransactionBehavior{TRequest,TResponse}"/> skips
/// these commands. A handler that has to do expensive, non-database work (parsing
/// an upload, BCrypt hashing thousands of passwords) can therefore finish that
/// work before it opens the transaction, instead of holding database locks for the
/// whole run.
/// </remarks>
/// <typeparam name="TResponse">The response type of the command.</typeparam>
public interface ISelfManagedTransactionCommand<out TResponse> : ICommand<TResponse>
{
}
