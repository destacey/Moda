namespace Moda.Common.Application.Requests.Goals.Commands;

public sealed record DeleteObjectiveCommand(Guid Id) : ICommand;
