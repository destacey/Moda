using Mapster;
using Moda.Common.Application.Employees.Dtos;
using Moda.Common.Application.Exceptions;
using Moda.Common.Application.Persistence;

namespace Moda.Common.Application.Employees.Queries;
public sealed record GetEmployeeQuery : IQuery<EmployeeDetailsDto?>
{
    public GetEmployeeQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
    public GetEmployeeQuery(int employeeKey)
    {
        EmployeeKey = employeeKey;
    }

    public Guid? EmployeeId { get; }
    public int? EmployeeKey { get; }
}

internal sealed class GetEmployeeQueryHandler : IQueryHandler<GetEmployeeQuery, EmployeeDetailsDto?>
{
    private readonly IModaDbContext _modaDbContext;
    private readonly ILogger<GetEmployeeQueryHandler> _logger;

    public GetEmployeeQueryHandler(IModaDbContext modaDbContext, ILogger<GetEmployeeQueryHandler> logger)
    {
        _modaDbContext = modaDbContext;
        _logger = logger;
    }

    public async Task<EmployeeDetailsDto?> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        var query = _modaDbContext.Employees.AsQueryable();

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(e => e.Id == request.EmployeeId.Value);
        }
        else if (request.EmployeeKey.HasValue)
        {
            query = query.Where(e => e.Key == request.EmployeeKey.Value);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No employee id or local id provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            throw exception;
        }

        return await query.ProjectToType<EmployeeDetailsDto>().FirstOrDefaultAsync(cancellationToken);
    }
}
