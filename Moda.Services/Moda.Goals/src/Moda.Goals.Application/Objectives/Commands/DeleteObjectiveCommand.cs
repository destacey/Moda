using Moda.Goals.Application.Persistence;

namespace Moda.Goals.Application.Objectives.Commands;
public sealed record DeleteObjectiveCommand(Guid Id) : ICommand;

internal sealed class DeleteObjectiveCommandHandler : ICommandHandler<DeleteObjectiveCommand>
{
    private readonly IGoalsDbContext _goalsDbContext;
    private readonly ILogger<DeleteObjectiveCommandHandler> _logger;

    public DeleteObjectiveCommandHandler(IGoalsDbContext goalsDbContext, ILogger<DeleteObjectiveCommandHandler> logger)
    {
        _goalsDbContext = goalsDbContext;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteObjectiveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var objective = await _goalsDbContext.Objectives.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
            if (objective is null)
                return Result.Failure($"Objective with id {request.Id} not found.");

            _goalsDbContext.Objectives.Remove(objective);

            await _goalsDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}