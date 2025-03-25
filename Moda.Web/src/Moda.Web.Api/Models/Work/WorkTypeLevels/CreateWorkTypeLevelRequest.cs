﻿using Moda.Work.Application.WorkTypeLevels.Commands;

namespace Moda.Web.Api.Models.Work.WorkTypeLevels;

public sealed record CreateWorkTypeLevelRequest
{
    /// <summary>The name of the work type level.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public string Name { get; set; } = default!;

    /// <summary>The description of the work type level.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    public CreateWorkTypeLevelCommand ToCreateWorkTypeLevelCommand()
    {
        return new CreateWorkTypeLevelCommand(Name, Description);
    }
}

public sealed class CreateWorkTypeLevelRequestValidator : CustomValidator<CreateWorkTypeLevelRequest>
{
    public CreateWorkTypeLevelRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
