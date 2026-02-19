using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Commands.Kpis;

namespace Moda.Web.Api.Models.Ppm.StrategicInitiatives;

public sealed record ManageStrategicInitiativeKpiCheckpointPlanRequest
{
    /// <summary>
    /// The ID of the strategic initiative to which this KPI belongs.
    /// </summary>
    public Guid StrategicInitiativeId { get; set; }

    /// <summary>
    /// The ID of the KPI.
    /// </summary>
    public Guid KpiId { get; set; }

    /// <summary>
    /// The list of planned KPI checkpoints.
    /// </summary>
    public List<KpiCheckpointPlanItemRequest> Checkpoints { get; set; } = [];

    public ManageStrategicInitiativeKpiCheckpointPlanCommand ToManageStrategicInitiativeKpiCheckpointPlanCommand()
    {
        var checkpoints = Checkpoints
            .Select(c => new StrategicInitiativeKpiCheckpointPlanItem(c.CheckpointId, c.TargetValue, c.AtRiskValue, c.CheckpointDate, c.DateLabel))
            .ToList();

        return new ManageStrategicInitiativeKpiCheckpointPlanCommand(StrategicInitiativeId, KpiId, checkpoints);
    }
}

public sealed record KpiCheckpointPlanItemRequest
{
    /// <summary>
    /// The ID of an existing checkpoint to update. Null for new checkpoints.
    /// </summary>
    public Guid? CheckpointId { get; set; }

    /// <summary>
    /// The target value for this checkpoint.
    /// </summary>
    public double TargetValue { get; set; }

    /// <summary>
    /// An optional at-risk threshold value for this checkpoint.
    /// </summary>
    public double? AtRiskValue { get; set; }

    /// <summary>
    /// The date and time of this checkpoint.
    /// </summary>
    public Instant CheckpointDate { get; set; }

    /// <summary>
    /// A short label describing this checkpoint (e.g. "Q1 2025").
    /// </summary>
    public string DateLabel { get; set; } = default!;
}

public sealed class ManageStrategicInitiativeKpiCheckpointPlanRequestValidator : AbstractValidator<ManageStrategicInitiativeKpiCheckpointPlanRequest>
{
    public ManageStrategicInitiativeKpiCheckpointPlanRequestValidator()
    {
        RuleFor(x => x.StrategicInitiativeId)
            .NotEmpty();

        RuleFor(x => x.KpiId)
            .NotEmpty();

        RuleFor(x => x.Checkpoints)
            .NotNull()
            .Must(checkpoints => checkpoints
                .Where(i => i.CheckpointId.HasValue)
                .Select(i => i.CheckpointId!.Value)
                .Distinct()
                .Count() == checkpoints.Count(i => i.CheckpointId.HasValue))
            .WithMessage("Checkpoint IDs must be unique.");

        RuleForEach(x => x.Checkpoints)
            .SetValidator(new KpiCheckpointPlanItemRequestValidator());
    }
}

public sealed class KpiCheckpointPlanItemRequestValidator : AbstractValidator<KpiCheckpointPlanItemRequest>
{
    public KpiCheckpointPlanItemRequestValidator()
    {
        RuleFor(x => x.CheckpointId)
            .NotEmpty()
            .When(x => x.CheckpointId.HasValue);

        RuleFor(x => x.TargetValue)
            .NotEmpty();

        RuleFor(x => x.AtRiskValue)
            .NotEqual(x => x.TargetValue)
            .When(x => x.AtRiskValue.HasValue)
            .WithMessage("At-risk value must differ from target value.");

        RuleFor(x => x.CheckpointDate)
            .NotEmpty();

        RuleFor(x => x.DateLabel)
            .NotEmpty()
            .MaximumLength(16);
    }
}
