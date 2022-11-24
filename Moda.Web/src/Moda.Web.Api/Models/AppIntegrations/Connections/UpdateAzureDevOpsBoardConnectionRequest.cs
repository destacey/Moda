using FluentValidation;

namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record UpdateAzureDevOpsBoardConnectionRequest
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the name of the connection.</summary>
    /// <value>The name of the connection.</value>
    public required string Name { get; set; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connection description.</value>
    public string? Description { get; set; }

    public UpdateAzureDevOpsBoardsConnectionCommand ToUpadateAzureDevOpsBoardsConnectionCommand()
        => new(Id, Name, Description);
}

public sealed class UpdateAzureDevOpsBoardConnectionRequestValidator : CustomValidator<UpdateAzureDevOpsBoardConnectionRequest>
{
    public UpdateAzureDevOpsBoardConnectionRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
