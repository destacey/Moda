using Moda.StrategicManagement.Application.StrategicThemes.Commands;

namespace Moda.Web.Api.Models.StrategicManagement.StrategicThemes;

public sealed record CreateStrategicThemeRequest
{

    /// <summary>
    /// The name of the strategic theme, highlighting its focus or priority.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// A detailed description of the strategic theme and its importance.
    /// </summary>
    public string Description { get; set; } = default!;

    public CreateStrategicThemeCommand ToCreateStrategicThemeCommand()
    {
        return new CreateStrategicThemeCommand(Name, Description);
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
    }
}
