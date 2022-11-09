using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class AreaService
{
    private readonly WorkItemTrackingHttpClient _witClient;
    private readonly string _projectName;
    private readonly ILogger<AreaService> _logger;

    internal AreaService(WorkItemTrackingHttpClient witClient, string projectName, ILogger<AreaService> logger)
    {
        _witClient = Guard.Against.Null(witClient);
        _projectName = Guard.Against.NullOrWhiteSpace(projectName);
        _logger = logger;
    }

    internal async Task<IReadOnlyList<WorkItemClassificationNode>> GetAreas(CancellationToken cancellationToken)
    {
        // TODO handle paging
        var rootNodes = await _witClient.GetRootNodesAsync(_projectName, cancellationToken: cancellationToken);

        if (rootNodes is null)
        {
            _logger.LogWarning("No root nodes found for project {ProjectName}", _projectName);
            return Array.Empty<WorkItemClassificationNode>();
        }

        // expect only one root node for the area path
        var rootAreaNode = rootNodes.Where(n => n.StructureType == TreeNodeStructureType.Area).FirstOrDefault();

        if (rootAreaNode is null)
        {
            _logger.LogWarning("No area node found for project {ProjectName}", _projectName);
            return Array.Empty<WorkItemClassificationNode>();
        }

        // TODO handle paging
        var areas = await _witClient.GetClassificationNodesAsync(_projectName, new int[] { rootAreaNode.Id }, cancellationToken: cancellationToken);
        
        _logger.LogDebug("{AreaCount} areas found for project {ProjectName}", areas.Count, _projectName);

        return areas;
    }
}
