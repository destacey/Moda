namespace Moda.Web.Api.Models.Work.WorkStatuses;

public sealed record CreateWorkStatusRequest
{
    /// <summary>The name of the work status.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work status.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    public CreateWorkStatusCommand ToCreateWorkStatusCommand()
    {
        return new CreateWorkStatusCommand(Name, Description);
    }
}

public sealed class CreateWorkStatusRequestValidator : CustomValidator<CreateWorkStatusRequest>
{
    public CreateWorkStatusRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
