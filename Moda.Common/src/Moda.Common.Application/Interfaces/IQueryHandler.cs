using MediatR;

namespace Moda.Common.Application.Interfaces;
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
}
