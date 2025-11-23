using System.Diagnostics.CodeAnalysis;

namespace Moda.AppIntegration.Domain.Models.OpenAI;

/// <summary>
/// 
/// </summary>
/// <param name="apiKey"></param>
/// <param name="modelName"></param>
/// <param name="baseUrl"></param>
/// <param name="defaultTemperature"></param>
/// <param name="defaultMaxOutputTokens"></param>
/// <param name="jsonModePreferred"></param>
[method: SetsRequiredMembers]
public sealed class OpenAIConnectionConfiguration(string apiKey, string modelName, string baseUrl = "https://api.openai.com/v1", double defaultTemperature = 0.1, int defaultMaxOutputTokens = 400, bool jsonModePreferred = true)
{
    /// <summary>
    /// Gets or sets the API key for Authorization: Bearer header to OpenAI.
    /// </summary>
    public required string ApiKey { get; set; } = apiKey.Trim();

    /// <summary>
    /// OpenAI base URL. Defaults to "https://api.openai.com/v1".
    /// </summary>
    public required string BaseUrl { get; set; } = baseUrl?.Trim() ?? "https://api.openai.com/v1";

    /// <summary>
    /// The OpenAI model name to use for this connection (e.g. "gpt-4o")
    /// </summary>
    public required string ModelName { get; set; } = modelName.Trim();

    /// <summary>
    /// OpenAI-Organization header gets set to this value
    /// </summary>
    public string? OrganizationId { get; set; }

    /// <summary>
    /// OpenAI-Project header gets set to this value
    /// </summary>
    public string? ProjectId { get; set; }

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

    #region Inferred Properties
    public (string Name, string Value) AuthorizationHeader => ("Authorization", $"Bearer {ApiKey}");
    public (string Name, string Value) OrganizationHeader => !string.IsNullOrWhiteSpace(OrganizationId) ? ("OpenAI-Organization", OrganizationId) : (string.Empty, string.Empty);
    public (string Name, string Value) ProjectHeader => !string.IsNullOrWhiteSpace(ProjectId) ? ("OpenAI-Project", ProjectId) : (string.Empty, string.Empty);
    public IEnumerable<(string Name, string Value)> Headers
        => new List<(string Name, string Value)>
        {
            AuthorizationHeader,
            OrganizationHeader,
            ProjectHeader
        }
        .Where(h => !string.IsNullOrWhiteSpace(h.Name));
    #endregion
}