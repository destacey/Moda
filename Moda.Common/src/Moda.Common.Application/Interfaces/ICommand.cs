using MediatR;

namespace Moda.Common.Application.Interfaces;
public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
