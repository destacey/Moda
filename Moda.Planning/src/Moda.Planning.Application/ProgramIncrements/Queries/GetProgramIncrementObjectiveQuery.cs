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

    public GetProgramIncrementObjectiveQuery(int localId, int objectiveLocalId)
    {
        LocalId = localId;
        ObjectiveLocalId = objectiveLocalId;
    }

    public Guid? Id { get; set; }
    public Guid? ObjectiveId { get; }
    public int? LocalId { get; set; }
    public int? ObjectiveLocalId { get; }
}

internal sealed class GetProgramIncrementObjectiveQueryHandler : IQueryHandler<GetProgramIncrementObjectiveQuery, ProgramIncrementObjectiveDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetProgramIncrementObjectiveQueryHandler> _logger;
    private readonly ISender _sender;

    public GetProgramIncrementObjectiveQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetProgramIncrementObjectiveQueryHandler> logger, ISender sender)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _sender = sender;
    }

    public async Task<ProgramIncrementObjectiveDetailsDto?> Handle(GetProgramIncrementObjectiveQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.ProgramIncrements.AsQueryable();

        if (request.Id.HasValue && request.ObjectiveId.HasValue)
        {
            query = query.Include(p => p.Objectives.Where(o => o.Id == request.ObjectiveId.Value))
                .ThenInclude(o => o.Team)
                .Where(p => p.Id == request.Id.Value);
        }
        else if (request.LocalId.HasValue && request.ObjectiveLocalId.HasValue)
        {
            query = query.Include(p => p.Objectives.Where(o => o.LocalId == request.ObjectiveLocalId.Value))
                .ThenInclude(o => o.Team)
                .Where(p => p.LocalId == request.LocalId.Value);
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
            || (request.ObjectiveLocalId.HasValue && programIncrement.Objectives.First().LocalId != request.ObjectiveLocalId))
            return null;

        // call the objective query handler
        var objective = await _sender.Send(new GetObjectiveForProgramIncrementQuery(programIncrement.Objectives.First().ObjectiveId, programIncrement.Id), cancellationToken);
        if (objective is null)
            return null;

        var piNavigation = NavigationDto.Create(programIncrement.Id, programIncrement.LocalId, programIncrement.Name);

        return ProgramIncrementObjectiveDetailsDto.Create(programIncrement.Objectives.First(), objective, piNavigation);
    }

    private void ThrowAndLogException(GetProgramIncrementObjectiveQuery request, string message)
    {
        var requestName = request.GetType().Name;
        var exception = new InternalServerException(message);

        _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}. {Message}", requestName, request, message);
        throw exception;
    }
}
