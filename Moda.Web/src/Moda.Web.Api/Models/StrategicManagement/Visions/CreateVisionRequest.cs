using Moda.StrategicManagement.Application.Visions.Commands;

namespace Moda.Web.Api.Models.StrategicManagement.Visions;

public sealed record CreateVisionRequest
{
    /// <summary>
    /// A concise statement describing the vision of the organization.
    /// </summary>
    public required string Description { get; set; }

    public CreateVisionCommand ToCreateVisionCommand()
    {
        return new CreateVisionCommand(Description);
    }
}

public sealed class CreateVisionRequestValidator : CustomValidator<CreateVisionRequest>
{
    public CreateVisionRequestValidator()
    {
        RuleFor(v => v.Description)
            .NotEmpty()
            .MaximumLength(3072);
    }
}
