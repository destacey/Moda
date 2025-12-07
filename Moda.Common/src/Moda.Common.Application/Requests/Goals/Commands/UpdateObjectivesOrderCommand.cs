namespace Moda.Common.Application.Requests.Goals.Commands;
public sealed record UpdateObjectivesOrderCommand(Dictionary<Guid, int?> Objectives) : ICommand;