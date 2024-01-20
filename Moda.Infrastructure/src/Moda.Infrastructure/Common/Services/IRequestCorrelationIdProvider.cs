namespace Moda.Infrastructure.Common.Services;
public interface IRequestCorrelationIdProvider : IScopedService
{
    string? RequestCorrelationId { get; }

    string CorrelationId { get; }
}
