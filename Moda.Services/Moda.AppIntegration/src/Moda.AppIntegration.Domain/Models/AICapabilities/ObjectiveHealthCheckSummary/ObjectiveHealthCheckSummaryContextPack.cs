using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Moda.AppIntegration.Domain.Models.AICapabilities.ObjectiveHealthCheckSummary;

public sealed class ObjectiveHealthCheckSummaryContextPack
{
    public required ObjectiveMetadata ObjectiveMetadata { get; set; }

    /// <summary>
    /// Work Item Context Selection Policy:
    /// 1. All Non-Closed Blocked items
    /// 2. 3–5 most recent Active items
    /// 3. 3–5 most recent Done items
    /// 4. Any stale Active (> 14 days since update)
    /// Cap 20 total lines (earlier rules take precedence)
    /// </summary>
    public required List<WorkItemMetadata> WorkItemMetadata { get; set; }

    // TODO: Depending on how we want to do this, it may make sense to have a contextpack base class/interface
    // that has this ContentHash property so other context packs can implement it as well. Additionally, having
    // object-specific serialization logic may be useful for the actual send over to the LLM. Finally, since we're
    // likely to observe few-shot and in-context learning patterns, we'll have to figure how this objects info + the
    // system prompt etc. all get composed together into the final payload.

    /// <summary>
    /// Field that generates a SHA 256 hash of the pertinent fields on the context pack to be used for caching purposes.
    /// </summary>
    public string ContentHash => GenerateContentHash();

    /// <summary>
    /// Generates a SHA-256 hash of the context pack content for caching and idempotency purposes.
    /// </summary>
    private string GenerateContentHash()
    {
        var contentForHashing = new
        {
            ObjectiveHash = ObjectiveMetadata.GetContentHash(),
            WorkItemHashes = WorkItemMetadata.OrderBy(w => w.Id).Select(w => w.GetContentHash()).ToList()
        };

        var jsonString = JsonSerializer.Serialize(contentForHashing, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var bytes = Encoding.UTF8.GetBytes(jsonString);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
    
}

public class ObjectiveMetadata
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Generates a SHA-256 hash of this objective's content for caching and comparison purposes.
    /// </summary>
    public string GetContentHash()
    {
        var contentForHashing = new
        {
            Id,
            Title,
            Description,
            StartDate,
            EndDate
        };

        var jsonString = JsonSerializer.Serialize(contentForHashing, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var bytes = Encoding.UTF8.GetBytes(jsonString);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}

public class WorkItemMetadata
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string? Description { get; set; }
    public required string State { get; set; }
    public DateTime? LastUpdated { get; set; }

    /// <summary>
    /// Generates a SHA-256 hash of this work item's content for caching and comparison purposes.
    /// </summary>
    public string GetContentHash()
    {
        var contentForHashing = new
        {
            Id,
            Title,
            Description,
            State,
            LastUpdated
        };

        var jsonString = JsonSerializer.Serialize(contentForHashing, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var bytes = Encoding.UTF8.GetBytes(jsonString);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
