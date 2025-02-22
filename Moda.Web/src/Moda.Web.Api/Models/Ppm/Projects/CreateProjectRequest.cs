using Moda.ProjectPortfolioManagement.Application.Portfolios.Command;
using Moda.ProjectPortfolioManagement.Application.Projects.Commands;

namespace Moda.Web.Api.Models.Ppm.Projects;

public sealed record CreateProjectRequest
{
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
    /// The date range of the project.
    /// </summary>
    public LocalDateRange? DateRange { get; set; }

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
        return new CreateProjectCommand(Name, Description, ExpenditureCategoryId, DateRange, PortfolioId, ProgramId, SponsorIds, OwnerIds, ManagerIds, StrategicThemeIds);
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
            .MaximumLength(1024);

        RuleFor(x => x.SponsorIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("SponsorIds cannot contain empty GUIDs.");

        RuleFor(x => x.OwnerIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("OwnerIds cannot contain empty GUIDs.");

        RuleFor(x => x.ManagerIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("ManagerIds cannot contain empty GUIDs.");
    }
}
