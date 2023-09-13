
using MediatR;
using Moda.Goals.Application.Objectives.Commands;
using Moda.Goals.Application.Objectives.Queries;

namespace Moda.Planning.Application.ProgramIncrements.Commands;

public sealed record DeleteProgramIncrementObjectiveCommand(Guid ProgramIncrementId, Guid ProgramIncrementObjectiveId) : ICommand;

internal sealed class DeleteProgramIncrementObjectiveCommandHandler : ICommandHandler<DeleteProgramIncrementObjectiveCommand>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ISender _sender;
    private readonly ILogger<DeleteProgramIncrementObjectiveCommandHandler> _logger;

    public DeleteProgramIncrementObjectiveCommandHandler(IPlanningDbContext planningDbContext, ISender sender, ILogger<DeleteProgramIncrementObjectiveCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _sender = sender;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteProgramIncrementObjectiveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var programIncrement = await _planningDbContext.ProgramIncrements
                .Include(pi => pi.Objectives.Where(o => o.Id == request.ProgramIncrementObjectiveId))
                .FirstOrDefaultAsync(p => p.Id == request.ProgramIncrementId, cancellationToken);
            if (programIncrement is null)
            {
                _logger.LogError("Program Increment {ProgramIncrementId} not found.", request.ProgramIncrementId);
                return Result.Failure($"Program Increment {request.ProgramIncrementId} not found.");
            }

            var piObjective = programIncrement.Objectives.FirstOrDefault(o => o.Id == request.ProgramIncrementObjectiveId);
            if (piObjective is null)
            {
                _logger.LogError("Program Increment Objective {ProgramIncrementObjectiveId} not found.", request.ProgramIncrementObjectiveId);
                return Result.Failure($"Program Increment Objective {request.ProgramIncrementObjectiveId} not found.");
            }

            var currentObjective = await _sender.Send(new GetObjectiveForProgramIncrementQuery(piObjective.ObjectiveId, programIncrement.Id), cancellationToken);
            if (currentObjective is null)
                return Result.Failure<int>($"Objective {request.ProgramIncrementObjectiveId} not found.");

            var deleteResult = programIncrement.DeleteObjective(request.ProgramIncrementObjectiveId);
            if (deleteResult.IsFailure)
            {
                _logger.LogError("Unable to delete Program Increment Objective {ProgramIncrementObjectiveId} from Program Increment {ProgramIncrementId}.  Error: {Error}", request.ProgramIncrementObjectiveId, request.ProgramIncrementId, deleteResult.Error);
                return Result.Failure($"Unable to remove Program Increment Objective {request.ProgramIncrementObjectiveId} from Program Increment {request.ProgramIncrementId}. Error: {deleteResult.Error}");
            }
            
            // TODO: this is a hack to ensure the PI objective is soft deleted.  We should be able to just remove it from the collection and save changes.
            _planningDbContext.Entry(piObjective).State = EntityState.Deleted;
            await _planningDbContext.SaveChangesAsync(cancellationToken);

            // TODO: this the correct order?  pi objective first, then objective?
            var deleteObjectiveResult = await _sender.Send(new DeleteObjectiveCommand(currentObjective.Id), cancellationToken);
            if (deleteObjectiveResult.IsFailure)
            {
                _logger.LogError("Unable to delete objective {ObjectiveId}.  Error: {Error}", currentObjective.Id, deleteObjectiveResult.Error);
                // don't return anything because we already deleted the PI objective
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Program Increment Objective {ProgramIncrementObjectiveId} from Program Increment {ProgramIncrementId}.", request.ProgramIncrementObjectiveId, request.ProgramIncrementId);
            return Result.Failure($"Error deleting Program Increment Objective {request.ProgramIncrementObjectiveId} from Program Increment {request.ProgramIncrementId}.");
        }
    }
}
