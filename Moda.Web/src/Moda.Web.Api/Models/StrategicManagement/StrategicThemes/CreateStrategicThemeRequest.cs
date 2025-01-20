using Moda.StrategicManagement.Application.StrategicThemes.Commands;
using Moda.StrategicManagement.Domain.Enums;

namespace Moda.Web.Api.Models.StrategicManagement.StrategicThemes;

public sealed record CreateStrategicThemeRequest
{

    /// <summary>
    /// The name of the strategic theme, highlighting its focus or priority.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// A detailed description of the strategic theme and its importance.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The current lifecycle state of the strategic theme (e.g., Active, Proposed, Archived).
    /// </summary>
    public int StateId { get; set; }

    public CreateStrategicThemeCommand ToCreateStrategicThemeCommand()
    {
        return new CreateStrategicThemeCommand(Name, Description, (StrategicThemeState)StateId);
    }
}

public sealed class CreateStrategicThemeRequestValidator : CustomValidator<CreateStrategicThemeRequest>
{
    public CreateStrategicThemeRequestValidator()
    {
        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(t => t.Description)
            .NotEmpty()
            .MaximumLength(1024);

        RuleFor(t => (StrategicThemeState)t.StateId)
            .IsInEnum()
            .WithMessage("The state is not valid.");
    }
}
