using Moda.Work.Application.WorkItems.Commands;

namespace Moda.Web.Api.Models.Work.Workspaces;

public class UpdateWorkItemProjectRequest
{
    public Guid WorkItemId { get; set; }
    public Guid? ProjectId { get; set; }

    public UpdateWorkItemProjectCommand ToUpdateWorkItemProjectCommand()
    {
        return new UpdateWorkItemProjectCommand(WorkItemId, ProjectId);
    }
}

public sealed class UpdateWorkItemProjectRequestValidator : CustomValidator<UpdateWorkItemProjectRequest>
{
    public UpdateWorkItemProjectRequestValidator()
    {
        RuleFor(c => c.WorkItemId)
            .NotEmpty();

        When(x => x.ProjectId.HasValue, () =>
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty();
        });
    }
}
