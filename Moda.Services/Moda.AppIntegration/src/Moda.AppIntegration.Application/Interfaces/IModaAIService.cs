using Moda.AppIntegration.Domain.Models.AICapabilities.ObjectiveHealthCheckSummary;

namespace Moda.AppIntegration.Application.Interfaces.AzureOpenAI;

public interface IModaAIService
{
    /// <summary>
    /// Gets the system ID for the given Azure OpenAI connection configuration.
    /// </summary>
    /// <param name="baseUrl"></param>
    /// <param name="apiKey"></param>
    /// <param name="deploymentName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<string>> GetSystemId(string baseUrl, string apiKey, string deploymentName, CancellationToken cancellationToken);

    /// <summary>
    /// Generates an Objective Health Check Summary based on the provided context pack.
    /// </summary>
    /// <param name="contextPack"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<ObjectiveHealthCheckSummaryResponse>> GenerateObjectiveHealthSummaryAsync(
        ObjectiveHealthCheckSummaryContextPack contextPack,
        CancellationToken cancellationToken);

    /// <summary>
    /// Tests the Azure OpenAI connection with the given parameters.
    /// </summary>
    /// <param name="baseUrl"></param>
    /// <param name="apiKey"></param>
    /// <param name="deploymentName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result> TestConnection(string baseUrl, string apiKey, string deploymentName, CancellationToken cancellationToken);
}