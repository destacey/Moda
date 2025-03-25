using Moda.ProjectPortfolioManagement.Application.Portfolios.Command;

namespace Moda.Web.Api.Models.Ppm.Portfolios;

public sealed record UpdatePortfolioRequest
{
    /// <summary>
    /// The unique identifier of the portfolio.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the portfolio.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// A detailed description of the portfolio’s purpose.
    /// </summary>
    public string Description { get; set; } = default!;

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

    public UpdateProjectPortfolioCommand ToUpdateProjectPortfolioCommand()
    {
        return new UpdateProjectPortfolioCommand(Id, Name, Description, SponsorIds, OwnerIds, ManagerIds);
    }
}

public sealed class UpdateProjectPortfolioRequestValidator : CustomValidator<UpdatePortfolioRequest>
{
    public UpdateProjectPortfolioRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(p => p.Description)
            .NotEmpty()
            .MaximumLength(1024);
    }
}
