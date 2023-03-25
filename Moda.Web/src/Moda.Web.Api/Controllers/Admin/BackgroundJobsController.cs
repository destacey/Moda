using Moda.Common.Application.BackgroundJobs;
using Moda.Organization.Application.Interfaces;

namespace Moda.Web.Api.Controllers.Admin;

[Route("api/admin/background-jobs")]
[ApiVersionNeutral]
[ApiController]
public class BackgroundJobsController : ControllerBase
{
    private readonly ILogger<BackgroundJobsController> _logger;
    private readonly IJobService _jobService;
    private readonly ISender _sender;
    private readonly IEmployeeService _employeeService;

    public BackgroundJobsController(ILogger<BackgroundJobsController> logger, IJobService jobService, ISender sender, IEmployeeService employeeService)
    {
        _logger = logger;
        _jobService = jobService;
        _sender = sender;
        _employeeService = employeeService;
    }

    [HttpGet("job-types")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.BackgroundJobs)]
    [OpenApiOperation("Get a list of all job types.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public ActionResult<IReadOnlyList<BackgroundJobDto>> GetRunningJobs()
    {
        var jobs = _jobService.GetRunningJobs();
        return Ok(jobs);
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Run, ApplicationResource.BackgroundJobs)]
    [OpenApiOperation("Run a background job.", "")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public IActionResult Run(int jobTypeId, CancellationToken cancellationToken)
    {
        switch ((BackgroundJobType)jobTypeId)
        {
            case BackgroundJobType.EmployeeImport:
                _jobService.Enqueue(() => _employeeService.SyncExternalEmployees(cancellationToken));
                break;
            default:
                return BadRequest();
        }
        return Accepted();
    }
}
