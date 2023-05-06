using Mapster;

namespace Moda.Organization.Application.Employees.Queries;
public sealed record GetEmployeeQuery : IQuery<EmployeeDetailsDto?>
{
    public GetEmployeeQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
    public GetEmployeeQuery(int employeeLocalId)
    {
        EmployeeLocalId = employeeLocalId;
    }

    public Guid? EmployeeId { get; }
    public int? EmployeeLocalId { get; }
}

internal sealed class GetEmployeeQueryHandler : IQueryHandler<GetEmployeeQuery, EmployeeDetailsDto?>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<GetEmployeeQueryHandler> _logger;

    public GetEmployeeQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<GetEmployeeQueryHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
    }

    public async Task<EmployeeDetailsDto?> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        var query = _organizationDbContext.Employees.AsQueryable();

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(e => e.Id == request.EmployeeId.Value);
        }
        else if (request.EmployeeLocalId.HasValue)
        {
            query = query.Where(e => e.LocalId == request.EmployeeLocalId.Value);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No employee id or local id provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            throw exception;
        }

        return await query
            .ProjectToType<EmployeeDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
