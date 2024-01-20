using MediatR;

namespace Moda.Common.Application.Behaviors;

public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            var customRequestTypes = new[] { typeof(IQuery<>), typeof(ICommand), typeof(ICommand<>) };
            var requestType = request.GetType().GetInterfaces().FirstOrDefault(i => i.IsGenericType && customRequestTypes.Contains(i.GetGenericTypeDefinition()));
            if (requestType is not null)
            {
                _logger.LogError(ex, "Moda Request: Unhandled Exception for Request {Name} {@Request}. Request Type: {RequestTypeName}", requestName, request, requestType.Name);

                var errorMessage = $"Moda Request: Unhandled Exception for Request {requestName} {request}";

                //if (requestType == typeof(ICommand) || requestType == typeof(ICommand<>))
                //{
                //    return requestType == typeof(ICommand)
                //        ? Result.Failure(errorMessage)
                //        : Result.Failure<TResponse>(errorMessage);
                //}
            }
            else
            {
                _logger.LogError(ex, "Moda Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);
            }

            throw;
        }
    }
}
