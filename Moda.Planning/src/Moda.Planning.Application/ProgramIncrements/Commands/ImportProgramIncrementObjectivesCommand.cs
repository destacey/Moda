using MediatR;
using Moda.Goals.Application.Objectives.Commands;
using Moda.Planning.Application.ProgramIncrements.Dtos;

namespace Moda.Planning.Application.ProgramIncrements.Commands;
public sealed record ImportProgramIncrementObjectivesCommand : ICommand
{
    public ImportProgramIncrementObjectivesCommand(IEnumerable<ImportProgramIncrementObjectiveDto> objectives)
    {
        Objectives = objectives.ToList();
    }

    public List<ImportProgramIncrementObjectiveDto> Objectives { get; }
}

public sealed class ImportProgramIncrementObjectivesCommandValidator : CustomValidator<ImportProgramIncrementObjectivesCommand>
{
    public ImportProgramIncrementObjectivesCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.Objectives)
            .NotNull()
            .NotEmpty();

        RuleForEach(o => o.Objectives)
            .NotNull()
            .SetValidator(new ImportProgramIncrementObjectiveDtoValidator());
    }
}

internal sealed class ImportProgramIncrementObjectivesCommandHandler : ICommandHandler<ImportProgramIncrementObjectivesCommand>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ISender _sender;
    private readonly ILogger<ImportProgramIncrementObjectivesCommandHandler> _logger;

    public ImportProgramIncrementObjectivesCommandHandler(IPlanningDbContext planningDbContext, ISender sender, ILogger<ImportProgramIncrementObjectivesCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _sender = sender;
        _logger = logger;
    }

    public async Task<Result> Handle(ImportProgramIncrementObjectivesCommand request, CancellationToken cancellationToken)
    {
        // TODO: allow individual records to fail and return a list of errors

        try
        {
            var piId = request.Objectives.First().ProgramIncrementId;

            var programIncrement = await _planningDbContext.ProgramIncrements
                .FirstOrDefaultAsync(p => p.Id == piId, cancellationToken);
            if (programIncrement is null)
                return Result.Failure<int>($"Program Increment {piId} not found.");

            if (programIncrement.ObjectivesLocked)
                return Result.Failure<int>($"Objectives are locked for Program Increment {piId}");

            var teams = await _planningDbContext.PlanningTeams
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var importedObjective in request.Objectives)
            {
                var team = teams.FirstOrDefault(t => t.Id == importedObjective.TeamId);
                if (team is null)
                    return Result.Failure<int>($"Team {importedObjective.TeamId} not found. (Record Id: {importedObjective.ImportId})");

                var objectiveResult = await _sender.Send(new ImportObjectiveCommand(
                    importedObjective.Name,
                    importedObjective.Description,
                    Goals.Domain.Enums.ObjectiveType.ProgramIncrement,
                    importedObjective.Status,
                    importedObjective.Progress,
                    importedObjective.TeamId,
                    importedObjective.ProgramIncrementId,
                    importedObjective.StartDate,
                    importedObjective.TargetDate,
                    importedObjective.ClosedDateUtc), cancellationToken);
                if (objectiveResult.IsFailure)
                    return Result.Failure<int>($"Unable to create objective. (Record Id: {importedObjective.ImportId}).  Error: {objectiveResult.Error}");

                var result = programIncrement.CreateObjective(team, objectiveResult.Value, importedObjective.IsStretch);
                if (result.IsFailure)
                {
                    var deleteResult = await _sender.Send(new DeleteObjectiveCommand(objectiveResult.Value), cancellationToken);
                    if (deleteResult.IsFailure)
                        _logger.LogError("Unable to delete objective. (Record Id: {importedObjective.ImportId}).  Error: {Error}", deleteResult.Error);

                    _logger.LogError("Unable to create PI objective. (Record Id: {importedObjective.ImportId}).  Error: {Error}", result.Error);
                    return Result.Failure<int>($"Unable to PI create objective. (Record Id: {importedObjective.ImportId}).  Error: {result.Error}");
                }

                await _planningDbContext.SaveChangesAsync(cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
