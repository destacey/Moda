using Moda.ProjectPortfolioManagement.Application.Portfolios.Command;

namespace Moda.Web.Api.Models.Ppm.Portfolios;

public sealed record CreatePortfolioRequest
{
    /// <summary>
    /// The name of the portfolio.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// A detailed description of the portfolio’s purpose.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The sponsors of the portfolio.
    /// </summary>
    public List<Guid>? SponsorIds { get; set; } = [];

    /// <summary>
    /// The owners of the portfolio.
    /// </summary>
    public List<Guid>? OwnerIds { get; set; } = [];

    /// <summary>
    /// The managers of the portfolio.
    /// </summary>
    public List<Guid>? ManagerIds { get; set; } = [];

    public CreateProjectPortfolioCommand ToCreateProjectPortfolioCommand()
    {
        return new CreateProjectPortfolioCommand(Name, Description, SponsorIds, OwnerIds, ManagerIds);
    }
}

public sealed class CreatePortfolioRequestValidator : CustomValidator<CreatePortfolioRequest>
{
    public CreatePortfolioRequestValidator()
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
