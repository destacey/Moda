﻿using Hangfire.Client;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;

namespace Moda.Infrastructure.BackgroundJobs;

public class LogJobFilter : IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
{
    private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

    public void OnCreating(CreatingContext context) =>
        _logger.InfoFormat("Creating a job based on method {0}...", context.Job.Method.Name);

    public void OnCreated(CreatedContext context) =>
        _logger.InfoFormat(
            "Job that is based on method {0} has been created with id {1}",
            context.Job.Method.Name,
            context.BackgroundJob?.Id);

    public void OnPerforming(PerformingContext context) =>
        _logger.InfoFormat("Starting to perform job {0}", context.BackgroundJob.Id);

    public void OnPerformed(PerformedContext context) =>
        _logger.InfoFormat("Job {0} has been performed", context.BackgroundJob.Id);

    public void OnStateElection(ElectStateContext context)
    {
        if (context.CandidateState is FailedState failedState)
        {
            _logger.WarnFormat(
                "Job '{0}' has been failed due to an exception {1}",
                context.BackgroundJob.Id,
                failedState.Exception);
        }
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction) =>
        _logger.InfoFormat(
            "Job {0} state was changed from {1} to {2}",
            context.BackgroundJob.Id,
            context.OldStateName,
            context.NewState.Name);

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) =>
        _logger.InfoFormat(
            "Job {0} state {1} was unapplied.",
            context.BackgroundJob.Id,
            context.OldStateName);
}