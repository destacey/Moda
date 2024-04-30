﻿using System.Linq.Expressions;
using Hangfire;
using Moda.Common.Application.BackgroundJobs;
using Moda.Common.Application.Enums;
using Moda.Web.Api.Interfaces;
using Moda.Web.Api.Models.Admin;

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

    [HttpPost("run")]
    [MustHavePermission(ApplicationAction.Run, ApplicationResource.BackgroundJobs)]
    [OpenApiOperation("Run a background job.", "")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public IActionResult Run(int jobTypeId, [FromServices] IJobManager jobManager, CancellationToken cancellationToken)
    {
        var jobType = (BackgroundJobType)jobTypeId;

        // TODO: should this code be moved to the manager?
        switch (jobType)
        {
            case BackgroundJobType.EmployeeSync:
                _jobService.Enqueue(() => jobManager.RunSyncExternalEmployees(cancellationToken));
                break;
            case BackgroundJobType.AzdoBoardsFullSync:
                _jobService.Enqueue(() => jobManager.RunSyncAzureDevOpsBoards(SyncType.Full, cancellationToken));
                break;
            case BackgroundJobType.AzdoBoardsDiffSync:
                _jobService.Enqueue(() => jobManager.RunSyncAzureDevOpsBoards(SyncType.Differential, cancellationToken));
                break;
            default:
                _logger.LogWarning("Unknown job type {jobType} requested", jobType);
                return BadRequest();
        }
        return Accepted();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Run, ApplicationResource.BackgroundJobs)]
    [OpenApiOperation("Create a recurring background job.", "")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateRecurringJobRequest request, [FromServices] IJobManager jobManager, CancellationToken cancellationToken)
    {
        _jobService.AddOrUpdate(request.JobId, GetMethodCall((BackgroundJobType)request.JobTypeId), () => request.CronExpression);

        return Accepted();

        Expression<Func<Task>> GetMethodCall(BackgroundJobType jobType)
        {
            return jobType switch
            {
                BackgroundJobType.EmployeeSync => () => jobManager.RunSyncExternalEmployees(cancellationToken),
                BackgroundJobType.AzdoBoardsFullSync => () => jobManager.RunSyncAzureDevOpsBoards(SyncType.Full, cancellationToken),
                BackgroundJobType.AzdoBoardsDiffSync => () => jobManager.RunSyncAzureDevOpsBoards(SyncType.Differential, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(jobType), jobType, "Unknown job type requested")
            };
        }
    }
}
