using MediatR;

namespace ATLAsBLL.CQRS;

/// <summary>
/// ICommand.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
