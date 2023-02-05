using Moda.Common.Application.BackgroundJobs;

namespace Moda.Web.Api.Controllers;

public class BackgroundJobsController : VersionNeutralApiController
{
    private readonly ILogger<BackgroundJobsController> _logger;
    private readonly IJobService _jobService;

    public BackgroundJobsController(ILogger<BackgroundJobsController> logger, IJobService jobService)
    {
        _logger = logger;
        _jobService = jobService;
    }

    [HttpGet("running")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.BackgroundJobs)]
    [OpenApiOperation("Get a list of running jobs.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<BackgroundJobDto>> GetRunningJobs()
    {
        var jobs = _jobService.GetRunningJobs();
        return Ok(jobs);
    }
}
