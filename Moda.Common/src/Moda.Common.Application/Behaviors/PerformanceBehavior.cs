using System.Diagnostics;
using MediatR;
using Serilog.Context;

namespace Moda.Common.Application.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : class, IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly ISerializerService _jsonSerializer;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, ISerializerService jsonSerializer)
    {
        _timer = new Stopwatch();

        _logger = logger;
        _jsonSerializer = jsonSerializer;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

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
