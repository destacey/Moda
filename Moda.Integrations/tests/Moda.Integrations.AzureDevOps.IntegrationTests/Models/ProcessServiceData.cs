using Microsoft.Extensions.Configuration;

namespace Moda.Integrations.AzureDevOps.IntegrationTests.Models;
public class ProcessServiceData : BaseConfiguration
{
    public readonly static string SectionName = "ProcessServiceData";

    public ProcessServiceData(IConfiguration configuration) : base(configuration, SectionName)
    {
    }

    public int ProcessListCount { get; init; }
    public Guid GetProcessId { get; init; }
    public string GetProcessName { get; init; } = string.Empty;
    public int GetProcessProjectsCount { get; init; }
    public Guid GetProcessProjectName { get; init; }
    public int GetProcessBacklogLevelsCount { get; init; }
}
