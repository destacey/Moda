using Moda.Planning.Application.Risks.Dtos;

namespace Moda.Planning.Application.Risks.Queries;
public sealed record GetRiskQuery : IQuery<RiskDetailsDto?>
{
    public GetRiskQuery(Guid riskId)
    {
        RiskId = riskId;
    }
    public GetRiskQuery(int riskKey)
    {
        RiskKey = riskKey;
    }

    public Guid? RiskId { get; }
    public int? RiskKey { get; }
}

internal sealed class GetRiskQueryHandler : IQueryHandler<GetRiskQuery, RiskDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetRiskQueryHandler> _logger;

    public GetRiskQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetRiskQueryHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
    }

    public async Task<RiskDetailsDto?> Handle(GetRiskQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.Risks
            .Include(r => r.Team)
            .Include(r => r.ReportedBy)
            .Include(r => r.Assignee)
            .AsQueryable();

        if (request.RiskId.HasValue)
        {
            query = query.Where(e => e.Id == request.RiskId.Value);
        }
        else if (request.RiskKey.HasValue)
        {
            query = query.Where(e => e.Key == request.RiskKey.Value);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No program increment id or local id provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            throw exception;
        }

        var risk = await query
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return risk?.Adapt<RiskDetailsDto>();
    }
}
