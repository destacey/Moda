using Moda.Common.Domain.Enums.Planning;

namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalSprint<TMetadata> where TMetadata : class
{
    string Id { get; }
    string Name { get; }
    DateTime? StartDate { get; }
    DateTime? EndDate { get; }
    SprintState State { get; }
    Guid? TeamId { get; }
    TMetadata Metadata { get; }
}
