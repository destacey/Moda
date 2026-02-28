using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.EstimationScales.Commands;

public sealed record UpdateEstimationScaleCommand(int Id, string Name, string? Description, List<UpdateEstimationScaleCommand.ScaleValue> Values) : ICommand
{
    public sealed record ScaleValue(string Value, int Order);
}

public sealed class UpdateEstimationScaleCommandValidator : CustomValidator<UpdateEstimationScaleCommand>
{
    public UpdateEstimationScaleCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Id)
            .GreaterThan(0);

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.Description)
            .MaximumLength(1024);

        RuleFor(c => c.Values)
            .NotEmpty()
            .Must(v => v.Count >= 2).WithMessage("An estimation scale must have at least 2 values.");

        RuleForEach(c => c.Values).ChildRules(v =>
        {
            v.RuleFor(x => x.Value).NotEmpty().MaximumLength(32);
            v.RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        });
    }
}

internal sealed class UpdateEstimationScaleCommandHandler(IPlanningDbContext planningDbContext, ILogger<UpdateEstimationScaleCommandHandler> logger) : ICommandHandler<UpdateEstimationScaleCommand>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly ILogger<UpdateEstimationScaleCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateEstimationScaleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var scale = await _planningDbContext.EstimationScales
                .Include(s => s.Values)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (scale is null)
                return Result.Failure("Estimation scale not found.");

            var updateResult = scale.Update(request.Name, request.Description);
            if (updateResult.IsFailure)
                return updateResult;

            var values = request.Values.Select(v => (v.Value, v.Order));
            var setValuesResult = scale.SetValues(values);
            if (setValuesResult.IsFailure)
                return setValuesResult;

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
