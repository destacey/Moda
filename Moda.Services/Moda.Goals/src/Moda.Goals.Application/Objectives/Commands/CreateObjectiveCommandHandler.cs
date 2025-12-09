using Moda.Common.Application.Requests.Goals.Commands;
using Moda.Goals.Application.Persistence;
using Moda.Goals.Domain.Models;

namespace Moda.Goals.Application.Objectives.Commands;

internal sealed class CreateObjectiveCommandHandler(IGoalsDbContext goalsDbContext, ILogger<CreateObjectiveCommandHandler> logger) : ICommandHandler<CreateObjectiveCommand, Guid>
{
    private readonly IGoalsDbContext _goalsDbContext = goalsDbContext;
    private readonly ILogger<CreateObjectiveCommandHandler> _logger = logger;

    public async Task<Result<Guid>> Handle(CreateObjectiveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var objective = Objective.Create(
                request.Name,
                request.Description,
                request.Type,
                request.OwnerId,
                request.PlanId,
                request.StartDate,
                request.TargetDate,
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
