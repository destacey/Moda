using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.EstimationScales.Commands;

public sealed record CreateEstimationScaleCommand(string Name, string? Description, List<CreateEstimationScaleCommand.ScaleValue> Values) : ICommand<int>
{
    public sealed record ScaleValue(string Value, int Order);
}

public sealed class CreateEstimationScaleCommandValidator : CustomValidator<CreateEstimationScaleCommand>
{
    public CreateEstimationScaleCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

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

internal sealed class CreateEstimationScaleCommandHandler(IPlanningDbContext planningDbContext, ILogger<CreateEstimationScaleCommandHandler> logger) : ICommandHandler<CreateEstimationScaleCommand, int>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly ILogger<CreateEstimationScaleCommandHandler> _logger = logger;

    public async Task<Result<int>> Handle(CreateEstimationScaleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var values = request.Values.Select(v => (v.Value, v.Order));

            var scaleResult = EstimationScale.Create(
                request.Name,
                request.Description,
                isPreset: false,
                values);

            if (scaleResult.IsFailure)
                return Result.Failure<int>(scaleResult.Error);

            await _planningDbContext.EstimationScales.AddAsync(scaleResult.Value, cancellationToken);
            await _planningDbContext.SaveChangesAsync(cancellationToken);

            return scaleResult.Value.Id;
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
