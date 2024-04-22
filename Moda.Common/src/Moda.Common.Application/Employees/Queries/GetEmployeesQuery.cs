using Mapster;
using Moda.Common.Application.Employees.Dtos;
using Moda.Common.Application.Persistence;

namespace Moda.Common.Application.Employees.Queries;
public sealed record GetEmployeesQuery(bool IncludeInactive = false) : IQuery<IReadOnlyList<EmployeeListDto>>;

internal sealed class GetEmployeesQueryHandler : IQueryHandler<GetEmployeesQuery, IReadOnlyList<EmployeeListDto>>
{
    private readonly IModaDbContext _modaDbContext;

    public GetEmployeesQueryHandler(IModaDbContext modaDbContext)
    {
        _modaDbContext = modaDbContext;
    }

    public async Task<IReadOnlyList<EmployeeListDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var query = _modaDbContext.Employees
            .AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        return await query.ProjectToType<EmployeeListDto>().ToListAsync(cancellationToken);
    }
}
