using FluentValidation;

namespace Moda.Web.Api.Models.AppIntegrations.Connectors;


public sealed record UpdateAzureDevOpsBoardConnectorRequest
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the name of the connector.</summary>
    /// <value>The name of the connector.</value>
    public required string Name { get; set; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connector description.</value>
    public string? Description { get; set; }

    public UpdateAzureDevOpsBoardsConnectorCommand ToUpadateAzureDevOpsBoardsConnectorCommand()
        => new(Id, Name, Description);
}

public sealed class UpdateAzureDevOpsBoardConnectorRequestValidator : CustomValidator<UpdateAzureDevOpsBoardConnectorRequest>
{
    public UpdateAzureDevOpsBoardConnectorRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
