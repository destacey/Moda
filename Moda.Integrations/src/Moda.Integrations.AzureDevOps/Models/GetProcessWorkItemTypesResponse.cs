namespace Moda.Integrations.AzureDevOps.Models;
internal sealed record GetProcessWorkItemTypesResponse
{
    public int Count { get; set; }
    public List<ProcessDto> Value { get; set; } = new();
}

internal sealed record ProcessWorkItemTypeDto
{
    public required string ReferenceName { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsDisabled { get; set; }
    public List<ProcessWorkItemStateDto> States { get; set; } = new();
    public List<ProcessWorkItemTypeBehaviorsDto> Behaviors { get; set; } = new();
}

internal sealed record ProcessWorkItemStateDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string StateCategory { get; set; }
}

internal sealed record ProcessWorkItemTypeBehaviorsDto
{
    public required ProcessWorkItemTypeBehaviorDto Behavior { get; set; }
    public bool IsDefault { get; set; }
}

internal sealed record ProcessWorkItemTypeBehaviorDto
{
    public required string Id { get; set; }
}
