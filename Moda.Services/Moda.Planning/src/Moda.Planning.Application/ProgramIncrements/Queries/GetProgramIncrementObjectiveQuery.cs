using MediatR;
using Moda.Common.Application.Dtos;
using Moda.Goals.Application.Objectives.Queries;
using Moda.Planning.Application.ProgramIncrements.Dtos;

namespace Moda.Planning.Application.ProgramIncrements.Queries;

public sealed record GetProgramIncrementObjectiveQuery : IQuery<ProgramIncrementObjectiveDetailsDto?>
{
    public GetProgramIncrementObjectiveQuery(Guid id, Guid objectiveId)
    {
        Id = id;
        ObjectiveId = objectiveId;
    }

    public GetProgramIncrementObjectiveQuery(int key, int objectiveKey)
    {
        Key = key;
        ObjectiveKey = objectiveKey;
    }

    public Guid? Id { get; set; }
    public Guid? ObjectiveId { get; }
    public int? Key { get; set; }
    public int? ObjectiveKey { get; }
}

internal sealed class GetProgramIncrementObjectiveQueryHandler : IQueryHandler<GetProgramIncrementObjectiveQuery, ProgramIncrementObjectiveDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetProgramIncrementObjectiveQueryHandler> _logger;
    private readonly ISender _sender;
    private readonly IDateTimeService _dateTimeService;

    public GetProgramIncrementObjectiveQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetProgramIncrementObjectiveQueryHandler> logger, ISender sender, IDateTimeService dateTimeService)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _sender = sender;
        _dateTimeService = dateTimeService;
    }

    public async Task<ProgramIncrementObjectiveDetailsDto?> Handle(GetProgramIncrementObjectiveQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.ProgramIncrements.AsQueryable();

        if (request.Id.HasValue && request.ObjectiveId.HasValue)
        {
            query = query
                .Include(p => p.Objectives.Where(o => o.Id == request.ObjectiveId.Value))
                    .ThenInclude(o => o.Team)
                .Include(p => p.Objectives.Where(o => o.Id == request.ObjectiveId.Value))
                    .ThenInclude(o => o.HealthCheck)
                .Where(p => p.Id == request.Id.Value);
        }
        else if (request.Key.HasValue && request.ObjectiveKey.HasValue)
        {
            query = query
                .Include(p => p.Objectives.Where(o => o.Key == request.ObjectiveKey.Value))
                    .ThenInclude(o => o.Team)
                .Include(p => p.Objectives.Where(o => o.Key == request.ObjectiveKey.Value))
                    .ThenInclude(o => o.HealthCheck)
                .Where(p => p.Key == request.Key.Value);
        }
        else
        {
            ThrowAndLogException(request, "No program increment id or local id provided.");
        }

        var programIncrement = await query
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(cancellationToken);
        if (programIncrement is null || programIncrement.Objectives.Count != 1
            || (request.ObjectiveId.HasValue && programIncrement.Objectives.First().Id != request.ObjectiveId) 
            || (request.ObjectiveKey.HasValue && programIncrement.Objectives.First().Key != request.ObjectiveKey))
            return null;

        // call the objective query handler
        var objective = await _sender.Send(new GetObjectiveForProgramIncrementQuery(programIncrement.Objectives.First().ObjectiveId, programIncrement.Id), cancellationToken);
        if (objective is null)
            return null;

        var piNavigation = NavigationDto.Create(programIncrement.Id, programIncrement.Key, programIncrement.Name);

        return ProgramIncrementObjectiveDetailsDto.Create(programIncrement.Objectives.First(), objective, piNavigation, _dateTimeService.Now);
    }

    private void ThrowAndLogException(GetProgramIncrementObjectiveQuery request, string message)
    {
        var requestName = request.GetType().Name;
        var exception = new InternalServerException(message);

        _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}. {Message}", requestName, request, message);
        throw exception;
    }
}
