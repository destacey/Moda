using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Serilog.Context;

namespace Moda.Common.Application.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : class, IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _timer = new Stopwatch();

        _logger = logger;
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

            using (LogContext.PushProperty("ApplicationRequestModel", JsonSerializer.Serialize(request)))
            {
                _logger.LogWarning("Long running request: {ApplicationRequestName} completed in {ApplicationElapsed} ms", requestName, elapsedMilliseconds);
            }
        }

        return response;
    }
}
