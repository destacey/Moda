using MediatR;

namespace Wayd.Common.Application.Interfaces;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
}
