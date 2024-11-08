using System.Diagnostics;
using MediatR;
using Serilog.Context;

namespace Moda.Common.Application.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, ISerializerService jsonSerializer) : IPipelineBehavior<TRequest, TResponse> where TRequest : class, IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger = logger;
    private readonly ISerializerService _jsonSerializer = jsonSerializer;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();

        var response = await next();

        var elapsedMilliseconds = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;

        if (elapsedMilliseconds > 700)
        {
            var requestName = typeof(TRequest).Name;

            using (LogContext.PushProperty("ApplicationRequestModel", _jsonSerializer.Serialize(request)))
            {
                _logger.LogWarning("Long running request: {AppRequestName} completed in {ApplicationElapsed} ms", requestName, elapsedMilliseconds);
            }
        }

        return response;
    }
}
