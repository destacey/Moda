using Moda.Common.Application.Requests.Goals.Commands;
using Moda.Goals.Application.Persistence;
using Moda.Goals.Domain.Models;

namespace Moda.Goals.Application.Objectives.Commands;

internal sealed class ImportObjectiveCommandHandler(IGoalsDbContext goalsDbContext, ILogger<ImportObjectiveCommandHandler> logger) : ICommandHandler<ImportObjectiveCommand, Guid>
{
    private readonly IGoalsDbContext _goalsDbContext = goalsDbContext;
    private readonly ILogger<ImportObjectiveCommandHandler> _logger = logger;

    public async Task<Result<Guid>> Handle(ImportObjectiveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var objective = Objective.

                Import(
                request.Name,
                request.Description,
                request.Type,
                request.Status,
                request.Progress,
                request.OwnerId,
                request.PlanId,
                request.StartDate,
                request.TargetDate,
                request.ClosedDate,
                request.Order
                );

            await _goalsDbContext.Objectives.AddAsync(objective, cancellationToken);

            await _goalsDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(objective.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<Guid>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
