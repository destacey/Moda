using Wayd.StrategicManagement.Application.Visions.Commands;

namespace Wayd.Web.Api.Models.StrategicManagement.Visions;

public sealed record CreateVisionRequest
{
    /// <summary>
    /// A concise statement describing the vision of the organization.
    /// </summary>
    public string Description { get; set; } = default!;

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
