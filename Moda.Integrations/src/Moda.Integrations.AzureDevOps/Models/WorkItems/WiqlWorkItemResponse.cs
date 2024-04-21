namespace Moda.Integrations.AzureDevOps.Models.WorkItems;
internal sealed record WiqlWorkItemResponse
{
    public int Id { get; set; }
    //public required string Url { get; set; } // not currently used
}
