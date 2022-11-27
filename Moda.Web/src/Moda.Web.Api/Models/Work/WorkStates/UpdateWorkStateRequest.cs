using FluentValidation;

namespace Moda.Web.Api.Models.Work.WorkStates;

public sealed record UpdateWorkStateRequest
{
    public int Id { get; set; }

    /// <summary>The description of the work state.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    public UpdateWorkStateCommand ToUpdateWorkStateCommand()
    {
        return new UpdateWorkStateCommand(Id, Description);
    }
}

public sealed class UpdateWorkStateRequestValidator : CustomValidator<UpdateWorkStateRequest>
{
    public UpdateWorkStateRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
