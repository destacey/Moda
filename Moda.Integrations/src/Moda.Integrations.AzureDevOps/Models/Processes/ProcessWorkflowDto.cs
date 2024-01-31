namespace Moda.Integrations.AzureDevOps.Models.Processes;
internal sealed record ProcessWorkflowItemDto
{
    public required string TypeReferenceName { get; set; }
    public required string TypeName { get; set; }
    public bool TypeIsDisabled { get; set; }
    public string? BacklogLevelId { get; set; }
    public Guid StateId { get; set; }
    public required string StateName { get; set; }
    public required string StateCategory { get; set; }
    public int StateOrder { get; set; }
    public bool StateIsDisabled { get; set; }
}
