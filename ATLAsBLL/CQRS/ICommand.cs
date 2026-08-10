using MediatR;

namespace ATLAsBLL.CQRS;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
