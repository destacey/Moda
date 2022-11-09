using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class IterationService
{
    private readonly WorkItemTrackingHttpClient _witClient;
    private readonly string _projectName;
    private readonly ILogger<IterationService> _logger;

    internal IterationService(WorkItemTrackingHttpClient witClient, string projectName, ILogger<IterationService> logger)
    {
        _witClient = Guard.Against.Null(witClient);
        _projectName = Guard.Against.NullOrWhiteSpace(projectName);
        _logger = logger;
    }

    internal async Task<IReadOnlyList<WorkItemClassificationNode>> GetIterations(CancellationToken cancellationToken)
    {
        // TODO handle paging
        var rootNodes = await _witClient.GetRootNodesAsync(_projectName, cancellationToken: cancellationToken);

        if (rootNodes is null)
        {
            _logger.LogWarning("No root nodes found for project {ProjectName}", _projectName);
            return Array.Empty<WorkItemClassificationNode>();
        }

        // expect only one root node for the iteration path
        var rootIterationNode = rootNodes.Where(n => n.StructureType == TreeNodeStructureType.Iteration).FirstOrDefault();

        if (rootIterationNode is null)
        {
            _logger.LogWarning("No iteration node found for project {ProjectName}", _projectName);
            return Array.Empty<WorkItemClassificationNode>();
        }

        // TODO handle paging
        var iterations = await _witClient.GetClassificationNodesAsync(_projectName, new int[] { rootIterationNode.Id }, cancellationToken: cancellationToken);
        
        _logger.LogDebug("{IterationCount} iterations found for project {ProjectName}", iterations.Count, _projectName);

        return iterations;
    }
}
