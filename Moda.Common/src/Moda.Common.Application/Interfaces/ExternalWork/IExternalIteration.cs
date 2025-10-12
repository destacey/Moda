using Moda.Common.Domain.Enums.Planning;

namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalIteration<TMetadata> where TMetadata : class
{
    string Id { get; }
    string Name { get; }
    Instant? Start { get; }
    Instant? End { get; }
    IterationState State { get; }
    Guid? TeamId { get; }
    TMetadata Metadata { get; }
}
