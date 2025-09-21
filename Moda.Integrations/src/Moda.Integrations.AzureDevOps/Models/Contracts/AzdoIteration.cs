using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums.Planning;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;

public sealed record AzdoIteration : IExternalSprint<AzdoIterationMetadata>
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public SprintState State { get; init; }
    public Guid? TeamId { get; init; }
    public required AzdoIterationMetadata Metadata { get; init; }
}
