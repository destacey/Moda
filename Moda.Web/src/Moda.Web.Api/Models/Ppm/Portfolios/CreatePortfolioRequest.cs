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

    public CreateProjectPortfolioCommand ToCreateProjectPortfolioCommand()
    {
        return new CreateProjectPortfolioCommand(Name, Description);
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
    }
}
