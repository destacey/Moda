using CSharpFunctionalExtensions;
using MediatR;

namespace Moda.Common.Application.Interfaces;
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse>
{
}
