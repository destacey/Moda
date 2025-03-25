using Moda.StrategicManagement.Application.Visions.Commands;

namespace Moda.Web.Api.Models.StrategicManagement.Visions;

public sealed record UpdateVisionRequest
{
    /// <summary>
    /// The unique identifier of the vision.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// A concise statement describing the vision of the organization.
    /// </summary>
    public string Description { get; set; } = default!;

    public UpdateVisionCommand ToUpdateVisionCommand()
    {
        return new UpdateVisionCommand(Id, Description);
    }
}

public sealed class UpdateVisionRequestValidator : CustomValidator<UpdateVisionRequest>
{
    public UpdateVisionRequestValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.Description)
            .NotEmpty()
            .MaximumLength(3072);
    }
}
