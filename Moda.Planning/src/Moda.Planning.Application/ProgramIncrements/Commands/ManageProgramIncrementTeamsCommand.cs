namespace Moda.Planning.Application.ProgramIncrements.Commands;

public sealed record ManageProgramIncrementTeamsCommand(Guid Id, IEnumerable<Guid> TeamIds) : ICommand;

internal sealed class ManageProgramIncrementTeamsCommandHandler : ICommandHandler<ManageProgramIncrementTeamsCommand>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<ManageProgramIncrementTeamsCommandHandler> _logger;

    public ManageProgramIncrementTeamsCommandHandler(IPlanningDbContext planningDbContext, ILogger<ManageProgramIncrementTeamsCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
    }

    public async Task<Result> Handle(ManageProgramIncrementTeamsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var programIncrement = await _planningDbContext.ProgramIncrements
                .Include(x => x.ProgramIncrementTeams)
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (programIncrement == null)
            {
                _logger.LogWarning("Program Increment with Id {ProgramIncrementId} not found.", request.Id);
                return Result.Failure($"Program Increment with Id {request.Id} not found.");
            }

            // TODO validate teams exist, currently they are in a different bounded context

            var result = programIncrement.ManageProgramIncrementTeams(request.TeamIds);
            if (result.IsFailure)
                return Result.Failure(result.Error);

            await _planningDbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {CommandName} command.", nameof(ManageProgramIncrementTeamsCommand));
            return Result.Failure($"Error handling {nameof(ManageProgramIncrementTeamsCommand)} command.");
        }
    }
}