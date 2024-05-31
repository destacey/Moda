using Moda.Work.Application.WorkTypeLevels.Commands;

namespace Moda.Web.Api.Models.Work.WorkTypeLevels;

public sealed record UpdateWorkTypeLevelRequest
{

    public int Id { get; set; }

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

    public UpdateWorkTypeLevelCommand ToUpdateWorkTypeLevelCommand()
    {
        return new UpdateWorkTypeLevelCommand(Id, Name, Description, Order);
    }
}

public sealed class UpdateWorkTypeLevelRequestValidator : CustomValidator<UpdateWorkTypeLevelRequest>
{
    public UpdateWorkTypeLevelRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
