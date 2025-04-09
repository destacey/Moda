using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Commands;

namespace Moda.Web.Api.Models.Ppm.StrategicInitiatives;

public sealed record ManageStrategicInitiativeProjectsRequest
{
    /// <summary>
    /// The ID of the strategic initiative to manage projects for.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The list of project IDs to be associated with the strategic initiative.
    /// </summary>
    public List<Guid> ProjectIds { get; set; } = [];

    public ManageStrategicInitiativeProjectsCommand ToManageStrategicInitiativeProjectsCommand()
    {
        return new ManageStrategicInitiativeProjectsCommand(Id, ProjectIds);
    }
}

public sealed class ManageStrategicInitiativeProjectsRequestValidator : AbstractValidator<ManageStrategicInitiativeProjectsRequest>
{
    public ManageStrategicInitiativeProjectsRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.ProjectIds)
            .NotNull()
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("ProjectIds cannot contain empty GUIDs.");
    }
}
