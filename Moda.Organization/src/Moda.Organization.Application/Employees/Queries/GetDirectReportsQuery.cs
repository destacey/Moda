using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.Employees.Queries;
public sealed record GetDirectReportsQuery(Guid EmployeeId) : IQuery<IReadOnlyList<EmployeeListDto>>;

internal sealed class GetDirectReportsQueryHandler : IQueryHandler<GetDirectReportsQuery, IReadOnlyList<EmployeeListDto>>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<GetDirectReportsQueryHandler> _logger;

    public GetDirectReportsQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<GetDirectReportsQueryHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<EmployeeListDto>> Handle(GetDirectReportsQuery request, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.Employees
            .Where(e => e.ManagerId == request.EmployeeId && e.IsActive)
            .ProjectToType<EmployeeListDto>()
            .ToListAsync(cancellationToken);
    }
}
