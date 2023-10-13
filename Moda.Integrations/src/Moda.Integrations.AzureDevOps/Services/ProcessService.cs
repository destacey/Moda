using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class ProcessService
{
    private readonly ProcessHttpClient _processClient;
    private readonly ILogger<ProcessService> _logger;

    public ProcessService(VssConnection connection, ILogger<ProcessService> logger)
    {
        _processClient = connection.GetClient<ProcessHttpClient>();
        _logger = logger;
    }

    public async Task<Result<List<Process>>> GetProcesses()
    {
        try
        {
            var processes = await _processClient.GetProcessesAsync();

            _logger.LogDebug("{ProcessCount} processes found ", processes.Count);

            return Result.Success(processes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processes from Azure DevOps");
            return Result.Failure<List<Process>>(ex.ToString());
        }
    }
}
