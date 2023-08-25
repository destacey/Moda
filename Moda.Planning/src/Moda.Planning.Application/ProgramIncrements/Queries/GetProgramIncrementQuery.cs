using Moda.Planning.Application.ProgramIncrements.Dtos;

namespace Moda.Planning.Application.ProgramIncrements.Queries;
public sealed record GetProgramIncrementQuery : IQuery<ProgramIncrementDetailsDto?>
{
    public GetProgramIncrementQuery(Guid programIncrementId)
    {
        ProgramIncrementId = programIncrementId;
    }
    public GetProgramIncrementQuery(int programIncrementKey)
    {
        ProgramIncrementKey = programIncrementKey;
    }

    public Guid? ProgramIncrementId { get; }
    public int? ProgramIncrementKey { get; }
}

internal sealed class GetProgramIncrementQueryHandler : IQueryHandler<GetProgramIncrementQuery, ProgramIncrementDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetProgramIncrementQueryHandler> _logger;
    private readonly IDateTimeService _dateTimeService;

    public GetProgramIncrementQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetProgramIncrementQueryHandler> logger, IDateTimeService dateTimeService)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<ProgramIncrementDetailsDto?> Handle(GetProgramIncrementQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.ProgramIncrements.AsQueryable();

        if (request.ProgramIncrementId.HasValue)
        {
            query = query.Where(e => e.Id == request.ProgramIncrementId.Value);
        }
        else if (request.ProgramIncrementKey.HasValue)
        {
            query = query.Where(e => e.Key == request.ProgramIncrementKey.Value);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No program increment id or local id provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            throw exception;
        }

        return await query
            .Select(p => ProgramIncrementDetailsDto.Create(p, _dateTimeService))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
