using MediatR;
using Moda.Common.Application.Dtos;
using Moda.Goals.Application.Objectives.Queries;
using Moda.Planning.Application.ProgramIncrements.Dtos;

namespace Moda.Planning.Application.ProgramIncrements.Queries;

public sealed record GetProgramIncrementObjectivesQuery : IQuery<IReadOnlyList<ProgramIncrementObjectiveListDto>>
{
    public GetProgramIncrementObjectivesQuery(Guid id, Guid? teamId)
    {
        Id = id;
        TeamId = teamId;
    }

    public GetProgramIncrementObjectivesQuery(int key, Guid? teamId)
    {
        Key = key;
        TeamId = teamId;
    }

    public Guid? Id { get; set; }
    public int? Key { get; set; }
    public Guid? TeamId { get; set; }
}

internal sealed class GetProgramIncrementObjectivesQueryHandler : IQueryHandler<GetProgramIncrementObjectivesQuery, IReadOnlyList<ProgramIncrementObjectiveListDto>>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetProgramIncrementObjectivesQueryHandler> _logger;
    private readonly ISender _sender;
    private readonly IDateTimeService _dateTimeService;

    public GetProgramIncrementObjectivesQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetProgramIncrementObjectivesQueryHandler> logger, ISender sender, IDateTimeService dateTimeService)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _sender = sender;
        _dateTimeService = dateTimeService;
    }

    public async Task<IReadOnlyList<ProgramIncrementObjectiveListDto>> Handle(GetProgramIncrementObjectivesQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.ProgramIncrements.AsQueryable();

        if (request.TeamId.HasValue)
        {
            var piTeamExists = await _planningDbContext.ProgramIncrements
                .AnyAsync(p => p.Id == request.Id && p.Teams.Any(t => t.TeamId == request.TeamId.Value), cancellationToken);
            if (!piTeamExists)
            {
                ThrowAndLogException(request, $"Program increment {request.Id} does not have team {request.TeamId}.");
            }

            query = query
                .Include(p => p.Objectives.Where(o => o.TeamId == request.TeamId.Value))
                    .ThenInclude(o => o.Team)
                .Include(p => p.Objectives.Where(o => o.TeamId == request.TeamId.Value))
                    .ThenInclude(o => o.HealthCheck);
        }
        else
        {
            query = query
                .Include(p => p.Objectives)
                    .ThenInclude(o => o.Team)
                .Include(p => p.Objectives)
                    .ThenInclude(o => o.HealthCheck);
        }

        if (request.Id.HasValue)
        {
            query = query.Where(p => p.Id == request.Id.Value);
        }
        else if (request.Key.HasValue)
        {
            query = query.Where(p => p.Key == request.Key.Value);
        }
        else
        {
            ThrowAndLogException(request, "No program increment id or local id provided.");
        }

        var programIncrement = await query
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(cancellationToken);
        if (programIncrement is null || !programIncrement.Objectives.Any())
            return new List<ProgramIncrementObjectiveListDto>();

        // call the objective query handler
        var teamIds = request.TeamId.HasValue ? new Guid[] { request.TeamId.Value } : null;
        var objectives = await _sender.Send(new GetObjectivesForProgramIncrementsQuery(new Guid[] { programIncrement.Id }, teamIds), cancellationToken);
        if (!objectives.Any() || programIncrement.Objectives.Count != objectives.Count)
            ThrowAndLogException(request, $"Error mapping objectives for program increment {programIncrement.Id}.");

        // map the list of objectives
        var piNavigation = NavigationDto.Create(programIncrement.Id, programIncrement.Key, programIncrement.Name);
        List<ProgramIncrementObjectiveListDto> piObjectives = new(objectives.Count);
        foreach (var piObjective in programIncrement.Objectives)
        {
            piObjectives.Add(ProgramIncrementObjectiveListDto.Create(piObjective, objectives.Single(o => o.Id == piObjective.ObjectiveId), piNavigation, _dateTimeService.Now));
        }

        return piObjectives;
    }

    private void ThrowAndLogException(GetProgramIncrementObjectivesQuery request, string message)
    {
        var requestName = request.GetType().Name;
        var exception = new InternalServerException(message);

        _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}. {Message}", requestName, request, message);
        throw exception;
    }
}
