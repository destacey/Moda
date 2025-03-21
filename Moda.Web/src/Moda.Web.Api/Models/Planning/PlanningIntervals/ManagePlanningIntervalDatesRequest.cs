using Moda.Planning.Application.PlanningIntervals.Commands;

namespace Moda.Web.Api.Models.Planning.PlanningIntervals;

public sealed record ManagePlanningIntervalDatesRequest
{
    public Guid Id { get; set; }

    /// <summary>Gets or sets the start.</summary>
    /// <value>The start.</value>
    public LocalDate Start { get; set; }

    /// <summary>Gets or sets the end.</summary>
    /// <value>The end.</value>
    public LocalDate End { get; set; }

    /// <summary>
    /// The iterations for the Planning Interval.
    /// </summary>
    public List<PlanningIntervalIterationUpsertRequest> Iterations { get; set; } = [];

    public ManagePlanningIntervalDatesCommand ToManagePlanningIntervalDatesCommand()
    {
        var iterations = Iterations.Select(i => i.ToPlanningIntervalIterationUpsertDto());
        return new ManagePlanningIntervalDatesCommand(Id, new LocalDateRange(Start, End), iterations);
    }
}

public sealed class ManagePlanningIntervalCalendarRequestValidator : CustomValidator<ManagePlanningIntervalDatesRequest>
{
    public ManagePlanningIntervalCalendarRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Id)
            .NotEmpty();

        RuleFor(t => t.Start)
            .NotNull();

        RuleFor(t => t.End)
            .NotNull()
            .Must((interval, end) => interval.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");
    }
}
