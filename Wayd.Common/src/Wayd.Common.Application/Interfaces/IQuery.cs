using MediatR;

namespace Wayd.Common.Application.Interfaces;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}
