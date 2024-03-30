using Moda.AppIntegration.Application.Interfaces;
using Moda.Common.Application.BackgroundJobs;
using Moda.Common.Application.Interfaces;

namespace Moda.Web.Api.Controllers.Admin;

[Route("api/admin/background-jobs")]
[ApiVersionNeutral]
[ApiController]
public class BackgroundJobsController : ControllerBase
{
    private readonly ILogger<BackgroundJobsController> _logger;
    private readonly IJobService _jobService;
    private readonly ISender _sender;

    public BackgroundJobsController(ILogger<BackgroundJobsController> logger, IJobService jobService, ISender sender)
    {
        _logger = logger;
        _jobService = jobService;
        _sender = sender;
    }

    [HttpGet("job-types")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.BackgroundJobs)]
    [OpenApiOperation("Get a list of all job types.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<BackgroundJobTypeDto>>> GetJobTypes(CancellationToken cancellationToken)
    {
        // TODO how do we determine what is active rather than returning all types
        var types = await _sender.Send(new GetBackgroundJobTypesQuery(), cancellationToken);
        return Ok(types.OrderBy(c => c.Order));
    }

    [HttpGet("running")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.BackgroundJobs)]
    [OpenApiOperation("Get a list of running jobs.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public ActionResult<IReadOnlyList<BackgroundJobDto>> GetRunningJobs()
    {
        var jobs = _jobService.GetRunningJobs();
        return Ok(jobs);
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Run, ApplicationResource.BackgroundJobs)]
    [OpenApiOperation("Run a background job.", "")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public IActionResult Run(int jobTypeId, 
        [FromServices] IEmployeeService employeeService,
        [FromServices] IAzureDevOpsBoardsSyncManager azdoBoardsSyncManager,
        CancellationToken cancellationToken)
    {
        var jobType = (BackgroundJobType)jobTypeId;
        _logger.LogInformation("Running job type {jobType}", jobType);

        switch (jobType)
        {
            case BackgroundJobType.EmployeeSync:
                _jobService.Enqueue(() => employeeService.SyncExternalEmployees(cancellationToken));
                break;
            case BackgroundJobType.AzdoBoardsSync:
                _jobService.Enqueue(() => azdoBoardsSyncManager.Sync(cancellationToken));
                break;
            default:
                _logger.LogWarning("Unknown job type {jobType} requested", jobType);
                return BadRequest();
        }
        return Accepted();
    }
}
