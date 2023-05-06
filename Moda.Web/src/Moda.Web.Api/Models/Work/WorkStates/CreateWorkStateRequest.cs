namespace Moda.Web.Api.Models.Work.WorkStates;

public sealed record CreateWorkStateRequest
{
    /// <summary>The name of the work state.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work state.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    public CreateWorkStateCommand ToCreateWorkStateCommand()
    {
        return new CreateWorkStateCommand(Name, Description);
    }
}

public sealed class CreateWorkStateRequestValidator : CustomValidator<CreateWorkStateRequest>
{
    public CreateWorkStateRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
