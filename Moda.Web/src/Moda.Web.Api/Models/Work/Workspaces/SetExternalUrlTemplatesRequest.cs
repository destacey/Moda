namespace Moda.Web.Api.Models.Work.Workspaces;

public sealed record SetExternalUrlTemplatesRequest(string? ExternalViewWorkItemUrlTemplate);

public sealed class SetExternalUrlTemplatesRequestValidator : CustomValidator<SetExternalUrlTemplatesRequest>
{
    public SetExternalUrlTemplatesRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.ExternalViewWorkItemUrlTemplate)
            .MaximumLength(256);
    }
}
