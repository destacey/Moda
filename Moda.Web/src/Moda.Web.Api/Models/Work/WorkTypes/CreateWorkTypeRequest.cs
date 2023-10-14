using Moda.Work.Application.WorkTypes.Commands;

namespace Moda.Web.Api.Models.Work.WorkTypes;

public sealed record CreateWorkTypeRequest
{
    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    public CreateWorkTypeCommand ToCreateWorkTypeCommand()
    {
        return new CreateWorkTypeCommand(Name, Description);
    }
}

public sealed class CreateWorkTypeRequestValidator : CustomValidator<CreateWorkTypeRequest>
{
    public CreateWorkTypeRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
