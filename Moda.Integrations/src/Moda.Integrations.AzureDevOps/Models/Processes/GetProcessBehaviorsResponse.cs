namespace Moda.Integrations.AzureDevOps.Models.Processes;
internal class GetProcessBehaviorsResponse
{
    public int Count { get; set; }
    public List<BehaviorDto> Value { get; set; } = new();
}
