using MediatR;

namespace Moda.Common.Application.Interfaces;
public interface IQuery<TResponse> : IRequest<TResponse>
{
}
