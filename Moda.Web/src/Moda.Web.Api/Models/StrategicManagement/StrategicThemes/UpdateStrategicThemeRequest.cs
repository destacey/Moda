using Moda.StrategicManagement.Application.StrategicThemes.Commands;

namespace Moda.Web.Api.Models.StrategicManagement.StrategicThemes;

public sealed record UpdateStrategicThemeRequest
{
    /// <summary>
    /// The unique identifier of the strategic theme.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the strategic theme, highlighting its focus or priority.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// A detailed description of the strategic theme and its importance.
    /// </summary>
    public string Description { get; set; } = default!;

    public UpdateStrategicThemeCommand ToUpdateStrategicThemeCommand()
    {
        return new UpdateStrategicThemeCommand(Id, Name, Description);
    }
}

public sealed class UpdateStrategicThemeRequestValidator : CustomValidator<UpdateStrategicThemeRequest>
{
    public UpdateStrategicThemeRequestValidator()
    {
        RuleFor(t => t.Id)
            .NotEmpty();

        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(t => t.Description)
            .NotEmpty()
            .MaximumLength(1024);
    }
}
