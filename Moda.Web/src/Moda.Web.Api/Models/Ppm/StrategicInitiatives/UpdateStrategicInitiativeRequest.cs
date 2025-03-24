using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Commands;

namespace Moda.Web.Api.Models.Ppm.StrategicInitiatives;

public class UpdateStrategicInitiativeRequest
{
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the strategic initiative.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// A detailed explanation of what the strategic initiative aims to achieve.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// The start date of the strategic initiative.
    /// </summary>
    public LocalDate Start { get; set; }

    /// <summary>
    /// The end date of the strategic initiative.
    /// </summary>
    public LocalDate End { get; set; }

    /// <summary>
    /// The sponsors of the strategic initiative.
    /// </summary>
    public List<Guid>? SponsorIds { get; set; } = [];

    /// <summary>
    /// The Owners of the strategic initiative.
    /// </summary>
    public List<Guid>? OwnerIds { get; set; } = [];

    public UpdateStrategicInitiativeCommand ToUpdateStrategicInitiativeCommand()
    {
        return new UpdateStrategicInitiativeCommand(Id, Name, Description, new LocalDateRange(Start, End), SponsorIds, OwnerIds);
    }
}

public sealed class UpdateStrategicInitiativeRequestValidator : AbstractValidator<UpdateStrategicInitiativeRequest>
{
    public UpdateStrategicInitiativeRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2048);

        RuleFor(x => x.Start)
            .NotNull();

        RuleFor(x => x.End)
            .NotNull()
            .Must((initiative, end) => initiative.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");

        RuleFor(x => x.SponsorIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("SponsorIds cannot contain empty GUIDs.");

        RuleFor(x => x.OwnerIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("OwnerIds cannot contain empty GUIDs.");
    }
}
