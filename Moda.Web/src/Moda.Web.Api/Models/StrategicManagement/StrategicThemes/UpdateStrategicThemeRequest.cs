﻿using Moda.Common.Domain.Enums.StrategicManagement;
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
    public required string Name { get; set; }

    /// <summary>
    /// A detailed description of the strategic theme and its importance.
    /// </summary>
    public required string Description { get; set; }

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
