namespace Moda.Integrations.AzureDevOps.Models.Processes;

internal sealed record ProcessWorkItemTypeBehaviorsDto
{
    public required ProcessWorkItemTypeBehaviorDto Behavior { get; set; }
    public bool IsDefault { get; set; }
}
