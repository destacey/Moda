using System.Diagnostics.CodeAnalysis;

namespace Moda.AppIntegration.Domain.Models.OpenAI;

/// <summary>
/// 
/// </summary>
/// <param name="apiKey"></param>
/// <param name="deploymentName"></param>
/// <param name="baseUrl"></param>
/// <param name="defaultTemperature"></param>
/// <param name="defaultMaxOutputTokens"></param>
/// <param name="jsonModePreferred"></param>
[method: SetsRequiredMembers]
public sealed class AzureOpenAIConnectionConfiguration(string apiKey, string deploymentName, string baseUrl, double defaultTemperature = 0.1, int defaultMaxOutputTokens = 400, bool jsonModePreferred = true)
{
    /// <summary>
    /// Gets or sets the API key for Azure OpenAI resource via ApiKeyCredential().
    /// </summary>
    public required string ApiKey { get; set; } = apiKey.Trim();

    /// <summary>
    /// Azure OpenAI resource URL.
    /// </summary>
    public required string BaseUrl { get; set; } = baseUrl.Trim();

    /// <summary>
    /// The OpenAI model name to use for this connection (e.g. "gpt-4o")
    /// </summary>
    public required string DeploymentName { get; set; } = deploymentName.Trim();

    #region Common AI Settings

    /// <summary>
    /// 
    /// </summary>
    public double DefaultTemperature { get; } = defaultTemperature;

    /// <summary>
    /// 
    /// </summary>
    public int DefaultMaxOutputTokens { get; } = defaultMaxOutputTokens;

    /// <summary>
    /// 
    /// </summary>
    public bool JsonModePreferred { get; } = jsonModePreferred;
    #endregion
}