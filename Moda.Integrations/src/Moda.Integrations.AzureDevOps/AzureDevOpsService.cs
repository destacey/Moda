using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Moda.Integrations.AzureDevOps.Services;

namespace Moda.Integrations.AzureDevOps;

public class AzureDevOpsService  // TODO add interface
{
    private readonly WorkItemTrackingHttpClient _witClient;
    private readonly ILogger<AzureDevOpsService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AzureDevOpsService(string organizationUrl, string token, ILogger<AzureDevOpsService> logger, IServiceProvider serviceProvider)
    {
        // connect to Azure DevOps Services
        VssBasicCredential credential = new(string.Empty, Guard.Against.NullOrWhiteSpace(token));
        VssConnection connection = new(new Uri(Guard.Against.NullOrWhiteSpace(organizationUrl)), credential);
        _witClient = connection.GetClient<WorkItemTrackingHttpClient>();

        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<IReadOnlyList<WorkItemClassificationNode>> GetAreas(string projectName, CancellationToken cancellationToken)
    {
        var logger = _serviceProvider.GetService(typeof(ILogger<AreaService>)) as ILogger<AreaService>;
        var areaService = new AreaService(_witClient, projectName, logger!);

        var areas = await areaService.GetAreas(cancellationToken);

        return areas; // return interface instead of concrete type
    }

    public async Task<IReadOnlyList<WorkItemClassificationNode>> GetIterations(string projectName, CancellationToken cancellationToken)
    {
        var logger = _serviceProvider.GetService(typeof(ILogger<IterationService>)) as ILogger<IterationService>;
        var iterationService = new IterationService(_witClient, projectName, logger!);

        var iterations = await iterationService.GetIterations(cancellationToken);

        return iterations; // return interface instead of concrete type
    }
}
