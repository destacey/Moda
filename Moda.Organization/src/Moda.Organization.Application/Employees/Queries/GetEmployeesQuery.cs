using Mapster;

namespace Moda.Organization.Application.Employees.Queries;
public sealed record GetEmployeesQuery : IQuery<IReadOnlyList<EmployeeListDto>>
{
    public GetEmployeesQuery(bool includeInactive = false)
    {
        IncludeInactive = includeInactive;
    }

    public bool IncludeInactive { get; }
}

internal sealed class GetEmployeesQueryHandler : IQueryHandler<GetEmployeesQuery, IReadOnlyList<EmployeeListDto>>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public GetEmployeesQueryHandler(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;
    }

    public async Task<IReadOnlyList<EmployeeListDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var query = _organizationDbContext.Employees.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        return await query.ProjectToType<EmployeeListDto>().ToListAsync(cancellationToken);
    }
}
