using CSharpFunctionalExtensions;
using MediatR;

namespace Moda.Common.Application.Interfaces;
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
