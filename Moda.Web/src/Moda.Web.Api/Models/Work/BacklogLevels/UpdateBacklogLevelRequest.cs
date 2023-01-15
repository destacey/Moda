using FluentValidation;
using Moda.Work.Application.BacklogLevels.Commands;

namespace Moda.Web.Api.Models.Work.BacklogLevels;

public sealed record UpdateBacklogLevelRequest
{

    public int Id { get; set; }

    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>
    /// The rank of the backlog level. The higher the number, the higher the level.
    /// </summary>
    /// <value>The rank.</value>
    public int Rank { get; set; }

    public UpdateBacklogLevelCommand ToUpdateBacklogLevelCommand()
    {
        return new UpdateBacklogLevelCommand(Id, Name, Description, Rank);
    }
}

public sealed class UpdateBacklogLevelRequestValidator : CustomValidator<UpdateBacklogLevelRequest>
{
    public UpdateBacklogLevelRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
