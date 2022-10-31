using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Organization.Application.Employees.Dtos;

namespace Moda.Organization.Application.Employees.Queries;
public sealed record GetEmployeesQuery : IQuery<IReadOnlyList<EmployeeListDto>>
{
    public GetEmployeesQuery(bool includeDisabled = false)
    {
        IncludeDisabled = includeDisabled;
    }

    public bool IncludeDisabled { get; }
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

        if (!request.IncludeDisabled)
            query = query.Where(e => e.IsActive);

        return await query.ProjectToType<EmployeeListDto>().ToListAsync(cancellationToken);
    }
}
