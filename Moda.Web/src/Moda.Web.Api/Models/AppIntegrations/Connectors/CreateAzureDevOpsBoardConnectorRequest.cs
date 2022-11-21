using FluentValidation;

namespace Moda.Web.Api.Models.AppIntegrations.Connectors;

public sealed record CreateAzureDevOpsBoardConnectorRequest
{
    /// <summary>Gets or sets the name of the connector.</summary>
    /// <value>The name of the connector.</value>
    public required string Name { get; set; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connector description.</value>
    public string? Description { get; set; }

    public CreateConnectorCommand ToCreateConnectorCommand()
        => new(Name, Description, ConnectorType.AzureDevOpsBoards);
}

public sealed class CreateAzureDevOpsBoardConnectorRequestValidator : CustomValidator<CreateAzureDevOpsBoardConnectorRequest>
{
    public CreateAzureDevOpsBoardConnectorRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
