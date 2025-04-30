using Moda.Work.Application.WorkItems.Commands;
using Moda.Work.Domain.Models;

namespace Moda.Web.Api.Models.Work.Workspaces;

public class UpdateWorkItemProjectRequest
{
    public string WorkItemKey { get; set; } = default!;
    public Guid? ProjectId { get; set; }

    public UpdateWorkItemProjectCommand ToUpdateWorkItemProjectCommand()
    {
        return new UpdateWorkItemProjectCommand(new WorkItemKey(WorkItemKey), ProjectId);
    }
}

public sealed class UpdateWorkItemProjectRequestValidator : CustomValidator<UpdateWorkItemProjectRequest>
{
    public UpdateWorkItemProjectRequestValidator()
    {
        RuleFor(c => c.WorkItemKey)
            .NotEmpty()
            .MaximumLength(256);

        When(x => x.ProjectId.HasValue, () =>
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty();
        });
    }
}
