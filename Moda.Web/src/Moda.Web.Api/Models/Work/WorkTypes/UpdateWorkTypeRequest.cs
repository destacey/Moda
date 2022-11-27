using FluentValidation;
using Moda.Work.Application.WorkTypes.Commands;

namespace Moda.Web.Api.Models.Work.WorkTypes;

public sealed record UpdateWorkTypeRequest
{
    public int Id { get; set; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    public UpdateWorkTypeCommand ToUpdateWorkTypeCommand()
    {
        return new UpdateWorkTypeCommand(Id, Description);
    }
}

public sealed class UpdateWorkTypeRequestValidator : CustomValidator<UpdateWorkTypeRequest>
{
    public UpdateWorkTypeRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}
