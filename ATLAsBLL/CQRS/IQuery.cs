using MediatR;

namespace ATLAsBLL.CQRS;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
