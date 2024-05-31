using Moda.Work.Application.WorkTypeLevels.Commands;

namespace Moda.Web.Api.Models.Work.WorkTypeLevels;

public sealed record CreateWorkTypeLevelRequest
{
    /// <summary>The name of the work type level.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work type level.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>
    /// The order of the work type level.
    /// </summary>
    /// <value>The order.</value>
    public int Order { get; set; }

    public CreateWorkTypeLevelCommand ToCreateWorkTypeLevelCommand()
    {
        return new CreateWorkTypeLevelCommand(Name, Description, Order);
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
