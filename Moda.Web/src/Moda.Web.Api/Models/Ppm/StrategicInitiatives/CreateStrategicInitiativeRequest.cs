using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Commands;

namespace Moda.Web.Api.Models.Ppm.StrategicInitiatives;

public class CreateStrategicInitiativeRequest
{
    /// <summary>
    /// The name of the strategic initiative.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// A detailed explanation of what the strategic initiative aims to achieve.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The start date of the strategic initiative.
    /// </summary>
    public LocalDate Start { get; set; }

    /// <summary>
    /// The end date of the strategic initiative.
    /// </summary>
    public LocalDate End { get; set; }

    /// <summary>
    /// The ID of the portfolio to which this strategic initiative belongs.
    /// </summary>
    public Guid PortfolioId { get; set; }

    /// <summary>
    /// The sponsors of the strategic initiative.
    /// </summary>
    public List<Guid>? SponsorIds { get; set; } = [];

    /// <summary>
    /// The Owners of the strategic initiative.
    /// </summary>
    public List<Guid>? OwnerIds { get; set; } = [];

    public CreateStrategicInitiativeCommand ToCreateStrategicInitiativeCommand()
    {
        return new CreateStrategicInitiativeCommand(Name, Description, new LocalDateRange(Start, End), PortfolioId, SponsorIds, OwnerIds);
    }
}

public sealed class CreateStrategicInitiativeRequestValidator : AbstractValidator<CreateStrategicInitiativeRequest>
{
    public CreateStrategicInitiativeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .MaximumLength(1024);

        RuleFor(x => x.Start)
            .NotNull();

        RuleFor(x => x.End)
            .NotNull()
            .Must((initiative, end) => initiative.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");

        RuleFor(x => x.PortfolioId)
            .NotEmpty();

        RuleFor(x => x.SponsorIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("SponsorIds cannot contain empty GUIDs.");

        RuleFor(x => x.OwnerIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("OwnerIds cannot contain empty GUIDs.");
    }
}
