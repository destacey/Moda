namespace Moda.Common.Application.Requests.Goals;
public sealed record UpdateObjectivesOrderCommand(Dictionary<Guid, int?> Objectives) : ICommand;