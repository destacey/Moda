namespace Moda.Common.Application.Interfaces;

/// <summary>
/// Represents a contract for a long-running request operation.  Requests implementing this interface will be ignored by the <see cref="PerformanceBehavior{TRequest, TResponse}"/> pipeline behavior.
/// </summary>
public interface ILongRunningRequest
{
}
