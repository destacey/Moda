using Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;
using NodaTime;

namespace Moda.Common.Domain.Events.ProjectPortfolioManagement;
public sealed record ProgramDetailsUpdatedEvent : DomainEvent, ISimpleProgram
{
    public ProgramDetailsUpdatedEvent(ISimpleProgram program, Instant timestamp)
    {
        Id = program.Id;
        Key = program.Key;
        Name = program.Name;
        Description = program.Description;

        Timestamp = timestamp;
    }

    public Guid Id { get; }
    public int Key { get; }
    public string Name { get; }
    public string Description { get; }
}