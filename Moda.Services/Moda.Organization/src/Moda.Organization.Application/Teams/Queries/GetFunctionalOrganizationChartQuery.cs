using NodaTime;

namespace Moda.Organization.Application.Teams.Queries;
public sealed record GetFunctionalOrganizationChartQuery(LocalDate? AsOfDate = null) : IQuery<FunctionalOrganizationChartDto>;

internal sealed class GetFunctionalOrganizationChartQueryHandler : IQueryHandler<GetFunctionalOrganizationChartQuery, FunctionalOrganizationChartDto>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<GetFunctionalOrganizationChartQueryHandler> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetFunctionalOrganizationChartQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<GetFunctionalOrganizationChartQueryHandler> logger, IDateTimeProvider dateTimeProvider)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<FunctionalOrganizationChartDto> Handle(GetFunctionalOrganizationChartQuery request, CancellationToken cancellationToken)
    {
        var asOfDate = request.AsOfDate ?? _dateTimeProvider.Now.InUtc().Date;


        return new FunctionalOrganizationChartDto();
    }
}
