using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moda.Integrations.AzureDevOps.Clients;
using Moda.Integrations.AzureDevOps.Models;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class ProcessService
{
    private readonly ProcessClient _processClient;
    private readonly ILogger<ProcessService> _logger;

    public ProcessService(string organizationUrl, string token, string apiVersion, ILogger<ProcessService> logger)
    {
        _processClient = new ProcessClient(organizationUrl, token, apiVersion);
        _logger = logger;
    }

    public async Task<Result<List<AzdoWorkProcess>>> GetProcesses(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _processClient.GetProcesses(cancellationToken);
            if (!response.IsSuccessful)
            {
                _logger.LogError("Error getting processes from Azure DevOps: {ErrorMessage}", response.ErrorMessage);
                return Result.Failure<List<AzdoWorkProcess>>(response.ErrorMessage);
            }

            _logger.LogDebug("{ProcessCount} processes found ", response.Data?.Count ?? 0);

            var processes = response.Data?.ToAzdoWorkProcesses() ?? new List<AzdoWorkProcess>();

            return Result.Success(processes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processes from Azure DevOps");
            return Result.Failure<List<AzdoWorkProcess>>(ex.ToString());
        }
    }    
}
