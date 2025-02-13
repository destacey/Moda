using Moda.ProjectPortfolioManagement.Application.Portfolios.Command;

namespace Moda.Web.Api.Models.Ppm.Portfolios;

public sealed record UpdatePortfolioRequest
{
    /// <summary>
    /// The unique identifier of the strategic theme.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the portfolio.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// A detailed description of the portfolio’s purpose.
    /// </summary>
    public required string Description { get; set; }

    public UpdateProjectPortfolioCommand ToUpdateProjectPortfolioCommand()
    {
        return new UpdateProjectPortfolioCommand(Id, Name, Description);
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
