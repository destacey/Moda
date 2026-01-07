using Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;
using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using NodaTime;

namespace Moda.Common.Domain.Events.ProjectPortfolioManagement;
public sealed record ProjectDetailsUpdatedEvent : DomainEvent, ISimpleProject
{
    public ProjectDetailsUpdatedEvent(ISimpleProject project, int expenditureCategoryId, Instant timestamp)
    {
        Id = project.Id;
        Key = project.Key;
        Name = project.Name;
        Description = project.Description;
        ExpenditureCategoryId = expenditureCategoryId;

        Timestamp = timestamp;
    }

    public Guid Id { get; init; }
    public ProjectKey Key { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public int ExpenditureCategoryId { get; init; }
}