using MediatR;

namespace ATLAsBLL.CQRS;

/// <summary>
/// IQuery.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
