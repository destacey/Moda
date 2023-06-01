using Mapster;
using Moda.Common.Application.Persistence;
using Moda.Common.Domain.Employees;

namespace Moda.Common.Application.Employees.Queries;
public sealed record GetEmployeeNumberMapQuery : IQuery<IReadOnlyList<EmployeeNumberMapDto>>
{
    public GetEmployeeNumberMapQuery(bool includeInactive = false)
    {
        IncludeInactive = includeInactive;
    }

    public bool IncludeInactive { get; }
}

internal sealed class GetEmployeeNumberMapQueryHandler : IQueryHandler<GetEmployeeNumberMapQuery, IReadOnlyList<EmployeeNumberMapDto>>
{
    private readonly IModaDbContext _modaDbContext;

    public GetEmployeeNumberMapQueryHandler(IModaDbContext modaDbContext)
    {
        _modaDbContext = modaDbContext;
    }

    public async Task<IReadOnlyList<EmployeeNumberMapDto>> Handle(GetEmployeeNumberMapQuery request, CancellationToken cancellationToken)
    {
        var query = _modaDbContext.Employees.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        return await query.ProjectToType<EmployeeNumberMapDto>().ToListAsync(cancellationToken);
    }
}

public sealed record EmployeeNumberMapDto : IMapFrom<Employee>
{
    /// <summary>Gets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the employee number.</summary>
    /// <value>The employee number.</value>
    public required string EmployeeNumber { get; set; }

    /// <summary>
    /// Indicates whether the employee is active or not.  
    /// </summary>
    public bool IsActive { get; set; }
}
