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
    public List<StrategicInitiativeKpiCheckpointPlanItem> Checkpoints { get; set; } = [];

    public ManageStrategicInitiativeKpiCheckpointPlanCommand ToManageStrategicInitiativeKpiCheckpointPlanCommand()
    {
        return new ManageStrategicInitiativeKpiCheckpointPlanCommand(StrategicInitiativeId, KpiId, Checkpoints);
    }
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
            .SetValidator(new StrategicInitiativeKpiCheckpointPlanItemValidator());
    }
}
