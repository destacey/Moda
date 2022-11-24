using FluentValidation;

namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record CreateAzureDevOpsBoardConnectionRequest
{
    /// <summary>Gets or sets the name of the connection.</summary>
    /// <value>The name of the connection.</value>
    public required string Name { get; set; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connection description.</value>
    public string? Description { get; set; }

    public CreateConnectionCommand ToCreateConnectionCommand()
        => new(Name, Description, Connector.AzureDevOpsBoards);
}

public sealed class CreateAzureDevOpsBoardConnectionRequestValidator : CustomValidator<CreateAzureDevOpsBoardConnectionRequest>
{
    public CreateAzureDevOpsBoardConnectionRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
