using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using Moda.ProjectPortfolioManagement.Application.Projects.Commands;

namespace Moda.Web.Api.Models.Ppm.Projects;

public sealed record CreateProjectRequest
{
    /// <summary>
    /// The name of the project.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// A detailed description of the project's purpose.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// The unique key for the project (2-20 uppercase alphanumeric characters).
    /// </summary>
    public string Key { get; set; } = default!;

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
    /// The ID of the portfolio to which this project belongs.
    /// </summary>
    public Guid PortfolioId { get; set; }

    /// <summary>
    /// The ID of the program to which this project belongs (optional).
    /// </summary>
    public Guid? ProgramId { get; set; }

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

    public CreateProjectCommand ToCreateProjectCommand()
    {
        var dateRange = Start is null || End is null ? null : new LocalDateRange((LocalDate)Start, (LocalDate)End);

        return new CreateProjectCommand(Name, Description, new ProjectKey(Key), ExpenditureCategoryId, dateRange, PortfolioId, ProgramId, SponsorIds, OwnerIds, ManagerIds, StrategicThemeIds);
    }
}

public sealed class CreateProjectRequestValidator : CustomValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(p => p.Description)
            .NotEmpty()
            .MaximumLength(2048);

        RuleFor(p => p.Key)
            .NotNull()
            .Matches(ProjectKey.Regex)
                .WithMessage("Invalid code format. Project keys are uppercase letters and numbers only, 2-20 characters.");

        RuleFor(p => p.ExpenditureCategoryId)
            .GreaterThan(0);

        RuleFor(p => p)
            .Must(p => (p.Start == null && p.End == null) || (p.Start != null && p.End != null))
            .WithMessage("Start and End must either both be null or both have a value.");

        RuleFor(p => p)
            .Must(p => p.Start == null || p.End == null || p.Start <= p.End)
            .WithMessage("End date must be greater than or equal to start date.");

        RuleFor(p => p.PortfolioId)
            .NotEmpty();

        RuleFor(p => p.ProgramId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("ProgramId cannot be an empty GUID.");

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
