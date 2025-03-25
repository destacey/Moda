using Moda.StrategicManagement.Application.Strategies.Commands;
using Moda.StrategicManagement.Domain.Enums;

namespace Moda.Web.Api.Models.StrategicManagement.Strategies;

public sealed record CreateStrategyRequest
{
    /// <summary>
    /// The concise statement describing the strategy and its purpose or focus area.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// A concise statement describing the strategy of the organization.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// The current status id of the strategy.
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// The start date of when the strategy became active.
    /// </summary>
    public LocalDate? Start { get; set; }

    /// <summary>
    /// The end date of when the strategy became archived.
    /// </summary>
    public LocalDate? End { get; set; }

    public CreateStrategyCommand ToCreateStrategyCommand()
    {
        FlexibleDateRange? dates = Start.HasValue ? new FlexibleDateRange(Start.Value, End) : null;
        return new CreateStrategyCommand(Name, Description, (StrategyStatus)StatusId, dates);
    }
}

public sealed class CreateStrategyRequestValidator : CustomValidator<CreateStrategyRequest>
{
    public CreateStrategyRequestValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .MaximumLength(1024);

        RuleFor(s => s.Description)
            .MaximumLength(3072);

        RuleFor(x => (StrategyStatus)x.StatusId)
            .IsInEnum();

        RuleFor(s => s.End)
            .Must((membership, end) => !end.HasValue || membership.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");
    }
}
