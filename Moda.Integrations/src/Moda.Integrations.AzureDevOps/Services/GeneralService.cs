using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moda.Integrations.AzureDevOps.Clients;
using Moda.Integrations.AzureDevOps.Models;

namespace Moda.Integrations.AzureDevOps.Services;

internal sealed class GeneralService(string organizationUrl, string token, string apiVersion, ILogger<GeneralService> logger)
{
    private readonly GeneralClient _generalClient = new(organizationUrl, token, apiVersion);
    private readonly ILogger<GeneralService> _logger = logger;

    public async Task<Result<ConnectionDataResponse?>> GetConnectionData(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _generalClient.GetConnectionData(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                _logger.LogError("Error getting connection data from Azure DevOps: {ErrorMessage}.", response.ErrorMessage);
                return null;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
            {
                _logger.LogError("The request was not authorized with Azure DevOps.");
                return Result.Failure<ConnectionDataResponse?>("The request was not authorized with Azure DevOps.");
            }

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Azure DevOps Instance ID: {InstanceId}", response.Data?.InstanceId);

            return response.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting connection data from Azure DevOps.");
            return Result.Failure<ConnectionDataResponse?>(ex.ToString());
        }
    }
}
