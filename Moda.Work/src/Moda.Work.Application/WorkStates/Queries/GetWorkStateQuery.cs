using Ardalis.GuardClauses;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moda.Common.Application.Exceptions;
using Moda.Work.Application.WorkStates.Dtos;

namespace Moda.Work.Application.WorkStates.Queries;
public sealed record GetWorkStateQuery : IQuery<WorkStateDto?>
{
    public GetWorkStateQuery(int id)
    {
        Id = id;
    }
    
    public GetWorkStateQuery(string name)
    {
        Name = Guard.Against.NullOrWhiteSpace(name).Trim();
    }
    
    public int? Id { get; }
    public string? Name { get; }
}

internal sealed class GetWorkStateQueryHandler : IQueryHandler<GetWorkStateQuery, WorkStateDto?>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly ILogger<GetWorkStateQueryHandler> _logger;

    public GetWorkStateQueryHandler(IWorkDbContext workDbContext, ILogger<GetWorkStateQueryHandler> logger)
    {
        _workDbContext = workDbContext;
        _logger = logger;
    }

    public async Task<WorkStateDto?> Handle(GetWorkStateQuery request, CancellationToken cancellationToken)
    {
        var query = _workDbContext.WorkStates.AsQueryable();

        if (request.Id.HasValue)
        {
            query = query.Where(s => s.Id == request.Id.Value);
        }
        else if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(s => s.Name == request.Name);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No work state id or name provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            throw exception;
        }

        return await query
            .ProjectToType<WorkStateDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
