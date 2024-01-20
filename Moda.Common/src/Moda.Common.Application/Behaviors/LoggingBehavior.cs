using MediatR;

namespace Moda.Common.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : class
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Processing request: {ApplicationRequestName}", requestName);

        TResponse response = await next();

        // THIS IS NOT CURRENTLY IN USE - only ICommand returns a Result in the reponse; IQuery does not
        // TODO: Update TResponse to "where TResponse : Result"
        // than add logging here for success/failure
        // than add this to the configure services

        return response;
    }
}
