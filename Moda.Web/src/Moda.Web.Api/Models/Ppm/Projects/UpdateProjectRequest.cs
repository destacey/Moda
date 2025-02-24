using Moda.ProjectPortfolioManagement.Application.Projects.Commands;

namespace Moda.Web.Api.Models.Ppm.Projects;

public sealed record UpdateProjectRequest
{
    /// <summary>
    /// The unique identifier of the project.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the project.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// A detailed description of the project’s purpose.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The ID of the expenditure category associated with the project.
    /// </summary>
    public int ExpenditureCategoryId { get; set; }

    /// <summary>
    /// The Project start date.
    /// </summary>
    public LocalDate? Start { get; set; }

    /// <summary>
    /// The Project end date.
    /// </summary>
    public LocalDate? End { get; set; }

    /// <summary>
    /// The sponsors of the project.
    /// </summary>
    public List<Guid>? SponsorIds { get; set; } = [];

    /// <summary>
    /// The owners of the project.
    /// </summary>
    public List<Guid>? OwnerIds { get; set; } = [];

    /// <summary>
    /// The managers of the project.
    /// </summary>
    public List<Guid>? ManagerIds { get; set; } = [];

    /// <summary>
    /// The strategic themes associated with this project.
    /// </summary>
    public List<Guid>? StrategicThemeIds { get; set; } = [];

    public UpdateProjectCommand ToUpdateProjectCommand()
    {
        var dateRange = Start is null || End is null ? null : new LocalDateRange((LocalDate)Start, (LocalDate)End);

        return new UpdateProjectCommand(Id, Name, Description, ExpenditureCategoryId, dateRange, SponsorIds, OwnerIds, ManagerIds, StrategicThemeIds);
    }
}

public sealed class UpdateProjectProjectRequestValidator : CustomValidator<UpdateProjectRequest>
{
    public UpdateProjectProjectRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(p => p.Description)
            .NotEmpty()
            .MaximumLength(2048);

        RuleFor(p => p.ExpenditureCategoryId)
            .GreaterThan(0);

        RuleFor(p => p)
            .Must(p => (p.Start == null && p.End == null) || (p.Start != null && p.End != null))
            .WithMessage("Start and End must either both be null or both have a value.");

        RuleFor(p => p)
            .Must(p => p.Start == null || p.End == null || p.Start <= p.End)
            .WithMessage("End date must be greater than or equal to start date.");

        RuleFor(x => x.SponsorIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("SponsorIds cannot contain empty GUIDs.");

        RuleFor(x => x.OwnerIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("OwnerIds cannot contain empty GUIDs.");

        RuleFor(x => x.ManagerIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("ManagerIds cannot contain empty GUIDs.");

        RuleFor(p => p.StrategicThemeIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("StrategicThemeIds cannot contain empty GUIDs.");
    }
}
